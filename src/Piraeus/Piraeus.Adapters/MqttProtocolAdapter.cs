using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
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

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        private MqttSession session;
        private bool disposed;
        private OrleansAdapter adapter;


        public override IChannel Channel { get; set; }


        public override void Init()
        {
            session.OnPublish += Session_OnPublish;
            session.OnSubscribe += Session_OnSubscribe;
            session.OnUnsubscribe += Session_OnUnsubscribe;
            session.OnDisconnect += Session_OnDisconnect; ;
            session.OnConnect += Session_OnConnect;
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
        }


        #region Dispose 
        protected void Disposing(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    adapter.Dispose();
                    session.Dispose();
                }

                disposed = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }
        
        #endregion
        

        #region Orleans Adapter Events
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            byte[] message = ProtocolTransition.ConvertToMqtt(session, e.Message);
            Task task = Channel.SendAsync(message);
            Task.WhenAll(task);
        }

        #endregion

        #region MQTT Session Events
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

        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            adapter.Dispose();
            Task task = Channel.CloseAsync();
            Task.WhenAll(task);
        }

        private void Session_OnUnsubscribe(object sender, MqttMessageEventArgs args)
        {
            UnsubscribeMessage msg = (UnsubscribeMessage)args.Message;
            foreach (var item in msg.Topics)
            {                
                Task task = Task.Factory.StartNew(async () =>
                {
                    MqttUri uri = new MqttUri(item.ToLowerInvariant());
                    if(await adapter.CanSubscribeAsync(uri.Resource, Channel.IsEncrypted))
                    {
                        await adapter.UnsubscribeAsync(uri.Resource);
                    }
                });

                Task.WhenAll(task);
            }
        }

        private List<string> Session_OnSubscribe(object sender, MqttMessageEventArgs args)
        {
            List<string> list = new List<string>();
            SubscribeMessage message = args.Message as SubscribeMessage;

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                Identity = session.Identity,
                Indexes = session.Indexes,
                IsEphemeral = true
            };

            //List<Task> taskList = new List<Task>();

            //foreach (var item in message.Topics)
            //{
            //    Task task = Task.Factory.StartNew(async () =>
            //    {
            //        MqttUri uri = new MqttUri(item.Key);
            //        string resourceUriString = uri.Resource;

            //        if (await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted))
            //        {
            //            string subscriptionUriString = await adapter.SubscribeAsync(resourceUriString, metadata);
            //            list.Add(resourceUriString);
            //        }
            //    });

            //    taskList.Add(task);
            //}

            //Task.WaitAll(taskList.ToArray());

            foreach (var item in message.Topics)
            {
                MqttUri uri = new MqttUri(item.Key);
                string resourceUriString = uri.Resource;

                if (adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted).Result)
                {
                    string subscriptionUriString = adapter.SubscribeAsync(resourceUriString, metadata).Result;
                    list.Add(resourceUriString);
                }
            }


            return list;
        }

        private void Session_OnPublish(object sender, MqttMessageEventArgs args)
        {
            PublishMessage message = args.Message as PublishMessage;

            MqttUri mqttUri = new MqttUri(message.Topic);

            Task task = Task.Factory.StartNew(async () =>
            {
                ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(mqttUri.Resource);

                if (await adapter.CanPublishAsync(mqttUri.Resource, Channel.IsEncrypted))
                {
                    EventMessage msg = new EventMessage(mqttUri.ContentType, mqttUri.Resource, ProtocolType.MQTT, message.Encode());
                    await adapter.PublishAsync(msg, session.Indexes);
                }
                else
                {
                    await Log.LogWarningAsync("Mqtt message cannot be published.");
                }
            });

            Task.WhenAll(task);
        }

        #endregion        

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;

            if (session.IsAuthenticated)
            {
                IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                session.Identity = decoder.Id;
                session.Indexes = decoder.Indexes;
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Exception error = null;
            
            try
            {
                MqttMessage msg = MqttMessage.DecodeMessage(e.Message);
                OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, e.Message));

                MqttMessageHandler handler = MqttMessageHandler.Create(session, msg);

                Task task = Task.Factory.StartNew(async () =>
                {
                    MqttMessage message = await handler.ProcessAsync();

                    if(message != null)
                    {
                        await Channel.SendAsync(message.Encode());
                    }
                });
                
                Task.WhenAll(task);
            }
            catch (AggregateException ae)
            {
                error = ae.Flatten().InnerException;
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                Task task = Log.LogErrorAsync("Mqtt receive error {0}", error.Message);
                Task.WhenAll(task);
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} state changed to {1}", Channel.Id, e.State);
            Task.WhenAll(task);
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} is retrying message.", e.ChannelId);
            Task.WhenAll(task);
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} sent message {1}", e.ChannelId, e.MessageId);
            Task.WhenAll(task);
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
            Task task = Log.LogErrorAsync("Channel {0} as error {1}", Channel.Id, e.Error.Message);
            Task.WhenAll(task);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(e.ChannelId));
            adapter.Dispose();
            session.Dispose();
        }

        #endregion

        #region Private methods

        
        #endregion


    }
}
