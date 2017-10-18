using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Security.Identity;

namespace Piraeus.Adapters
{
    public class MqttProtocolAdapter : ProtocolAdapter
    {
        public MqttProtocolAdapter(MqttConfig config, IChannel channel)
        {
            session = new MqttSession(config);
            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnSent += Channel_OnSent;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnStateChange += Channel_OnStateChange;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;
        }

        public override event ProtocolAdapterCloseHandler OnClose;
        public override event ProtocolAdapterErrorHandler OnError;
        public override IChannel Channel { get; set; }
        private MqttSession session;
        private bool disposed;
        private string nameIdentity;
        private List<KeyValuePair<string, string>> claimIndexes;
        private OrleansAdapter adapter;

        public override void Init()
        {
            session.OnPublish += Session_OnPublish;
            session.OnSubscribe += Session_OnSubscribe;
            session.OnUnsubscribe += Session_OnUnsubscribe;
            session.OnDisconnect += Session_OnDisconnect;
            session.OnConnect += Session_OnConnect;
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
        }

        



        #region Dispose code
        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {
                disposed = true;
                session.Dispose(); 
            }
        }

        #endregion

        #region Session Events
        private void Session_OnConnect(object sender, MqttConnectionArgs args)
        {
            if (args.Code == ConnectAckCode.ConnectionAccepted)
            {
                Task t = adapter.LoadDurableSubscriptionsAsync(session.Identity);
                Task.WhenAll(t);
            }

            Task task = Log.LogInfoAsync("Mqtt session is connected for channel {0}", Channel.Id);
            Task.WhenAll(task);
        }

        private List<string> Session_OnSubscribe(object sender, MqttMessageEventArgs args)
        {
            List<string> list = new List<string>();
            SubscribeMessage message = (SubscribeMessage)args.Message;            
            Task<List<string>> task = SubscribeAsync(message);
            Task.WhenAll<List<string>>(task);
            return task.Result;
        }
        

        private void Session_OnUnsubscribe(object sender, MqttMessageEventArgs args)
        {
            UnsubscribeMessage msg = (UnsubscribeMessage)args.Message;
            Task task = UnsubscribeAsync(msg);
            Task.WhenAll(task);
        }
        
        private void Session_OnPublish(object sender, MqttMessageEventArgs args)
        {
            //publish to resource
            PublishMessage message = (PublishMessage)args.Message;
            Task task = PublishAsync(message);
            Task.WhenAll(task);            
        }
        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            //clean up observers and remove ephemeral subscriptions
            //close the channel
            adapter.Dispose();
            Task task = Channel.CloseAsync();
            Task.WhenAll(task);
        }

        #region private async pub,sub,unsub
        private async Task PublishAsync(PublishMessage message)
        {
            MqttUri mqttUri = new MqttUri(message.Topic);
            IResource resource = await GraphManager.GetResourceAsync(mqttUri.Resource);
            ResourceMetadata metadata = await resource.GetMetadataAsync();            

            if (await CanPublishAsync(metadata))
            {
                EventMessage msg = new EventMessage(mqttUri.ContentType, mqttUri.Resource, ProtocolType.MQTT, message.Encode());
                if (mqttUri.Indexes == null)
                {
                    await resource.PublishAsync(msg);
                }
                else
                {
                    await resource.PublishAsync(msg, new List<KeyValuePair<string, string>>(mqttUri.Indexes));
                }
            }
            else
            {
                await Log.LogWarningAsync("Publish failed.");
            }
        }
        private async Task<List<string>> SubscribeAsync(SubscribeMessage message)
        {
            List<string> list = new List<string>();

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                Identity = session.Identity,
                Indexes = session.Indexes,
                IsEphemeral = true
            };

            foreach (var item in message.Topics)
            {
                MqttUri uri = new MqttUri(item.Key);
                string resourceUriString = uri.Resource;

                if (await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted))
                {
                    string subscriptionUriString = await adapter.SubscribeAsync(resourceUriString, metadata);
                    list.Add(resourceUriString);                
                }
            }

            return list;
        }
        private async Task UnsubscribeAsync(UnsubscribeMessage message)
        {
            foreach(var item in message.Topics)
            {
                MqttUri uri = new MqttUri(item.ToLower(CultureInfo.InvariantCulture));

                if(await adapter.CanSubscribeAsync(uri.Resource, Channel.IsEncrypted))
                {
                    await adapter.UnsubscribeAsync(uri.Resource);
                }                
            }
        }

        #endregion

        #endregion

        #region Adapter observe event
        
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            //convert message to protocol and send on channel
            byte[] message = ProtocolTransition.ConvertToMqtt(session, e.Message);
            Task task = Channel.SendAsync(message);
            Task.WhenAll(task);
        }

        #endregion

        

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;

            if (session.IsAuthenticated)
            {
                IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                session.Identity = decoder.Id;
                session.Indexes = decoder.Indexes;
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            Exception error = null;

            try
            {
                MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
                Task task = ReceiveAsync(msg);
                Task.WhenAll(task);
            }
            catch(AggregateException ae)
            {
                error = ae.Flatten().InnerException;
            }
            catch(Exception ex)
            {
                error = ex;
            }

            if(error != null)
            {
                Task task = Log.LogErrorAsync("Mqtt receive error {0}", error.Message);
                Task.WhenAll(task);
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
            }
        }

        private async Task ReceiveAsync(MqttMessage message)
        {
            Exception error = null;            
            MqttMessageHandler handler = MqttMessageHandler.Create(session, message);
            MqttMessage response = await handler.ProcessAsync();
            
            if(response != null)
            {
                try
                {
                    await Channel.SendAsync(response.Encode());
                }
                catch(AggregateException ae)
                {
                    error = ae.Flatten().InnerException;                    
                }
                catch(Exception ex)
                {
                    error = ex;
                }

                if(error != null)
                {
                    await Log.LogErrorAsync("Mqtt send error {0}", error.Message);
                }
            }      
            
            
            
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} state changed to {1}", Channel.Id, args.State);
            Task.WhenAll(task);
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} is retrying message.", args.ChannelId);
            Task.WhenAll(task);
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} sent message {1}", args.ChannelId, args.MessageId);
            Task.WhenAll(task);
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            Task task = Log.LogErrorAsync("Channel {0} as error {1}", Channel.Id, args.Error.Message);
            Task.WhenAll(task);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            adapter.Dispose();
        }

        #endregion

        #region Utilities

        

        public async Task<bool> CanSubscribeAsync(ResourceMetadata metadata)
        {
            if (metadata == null)
            {
                await Log.LogWarningAsync("Subscribe resource metadata is null.");
                return false;
            }

            if (!metadata.Enabled)
            {
                await Log.LogWarningAsync("Subscribe resource {0} is disabled.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                await Log.LogWarningAsync("Subscribe resource {0} has expired.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !Channel.IsEncrypted)
            {
                await Log.LogWarningAsync("Subscribe resource {0} requires an encrypted channel.  Channel {1} is not encrypted.", metadata.ResourceUriString, Channel.Id);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to subscribe/unsubscribe for resource {1}", nameIdentity, metadata.ResourceUriString);
            }

            return authz;
        }

        private async Task<bool> CanPublishAsync(ResourceMetadata metadata)
        {
            if(metadata == null)
            {
                await Log.LogWarningAsync("Publish resource metadata is null.");
                return false;
            }

            if (!metadata.Enabled)
            {
                await Log.LogWarningAsync("Publish resource {0} is disabled.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                await Log.LogWarningAsync("Publish resource {0} has expired.", metadata.ResourceUriString);
                return false;
            }

            if(metadata.RequireEncryptedChannel && !Channel.IsEncrypted)
            {
                await Log.LogWarningAsync("Publish resource {0} requires an encrypted channel.  Channel {1} is not encrypted.", metadata.ResourceUriString, Channel.Id);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if(!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to publish to resource {1}", nameIdentity, metadata.ResourceUriString);                
            }

            return authz;
        }

      
        #endregion





    }
    
}
