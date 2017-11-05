using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Piraeus.Configuration.Settings;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Security.Identity;

namespace Piraeus.Adapters
{
    public class RestProtocolAdapter : ProtocolAdapter
    {
        public RestProtocolAdapter(PiraeusConfig config, IChannel channel)
        {
            this.config = config;
            adapter = new OrleansAdapter();
            Channel = channel;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
        }
        

        public override IChannel Channel { get; set; }
        private PiraeusConfig config;
        private OrleansAdapter adapter;
        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        private bool disposedValue;

        public override void Init()
        {
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
            Task task = Channel.OpenAsync();
            Task.WhenAll(task);
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            byte[] payload = ProtocolTransition.ConvertToHttp(e.Message);
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(e.Message.ResourceUri, e.Message.ContentType, e.Message.Message));
        }

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            if(!Channel.IsAuthenticated)  //requires channel authentication
            {
                Task logTask = Log.LogErrorAsync("Channel {0} not authenticated.", Channel.Id);
                Task.WhenAll(logTask);

                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated.")));
                Task ctask = Channel.CloseAsync();
                Task.WhenAll(ctask);
                return;
            }

            if (args.Message.Method != HttpMethod.Post && args.Message.Method != HttpMethod.Get)
            {
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Rest protocol adapter requires GET or POST only.")));               
            }

            MessageUri uri = new MessageUri(args.Message);

            IdentityDecoder decoder = new IdentityDecoder(config.Identity.Client.IdentityClaimType, config.Identity.Client.Indexes);

            HttpRequestMessage request = (HttpRequestMessage)args.Message;

            if (request.Method == HttpMethod.Get)
            {
                foreach (var item in uri.Subscriptions)
                {
                    Task task = SubscribeAsync(item, decoder.Id, decoder.Indexes);
                    Task.WhenAll(task);
                }
            }

            if (request.Method == HttpMethod.Post)
            {
                EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, request.Content.ReadAsByteArrayAsync().Result);
                try
                {
                    List<KeyValuePair<string, string>> indexList = uri.Indexes == null ? null : new List<KeyValuePair<string, string>>(uri.Indexes);
                    Task task = PublishAsync(message, indexList);
                    Task.WhenAll(task);
                    Task final = Channel.CloseAsync();
                    Task.WhenAll(final);
                }
                catch(AggregateException ae)
                {

                }
                catch(Exception ex)
                {

                }
            }
            

        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {            
        }


        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
        }
        #endregion


        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    adapter.Dispose();
                }

                disposedValue = true;
            }
        }

        public override void Dispose()
        {

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion


        private async Task PublishAsync(EventMessage message, List<KeyValuePair<string,string>> indexes = null)
        {
            if(!await adapter.CanPublishAsync(message.ResourceUri, Channel.IsEncrypted))
            {
                return;
            }

            await adapter.PublishAsync(message, indexes);
        }

        private async Task SubscribeAsync(string resourceUriString, string identity, List<KeyValuePair<string, string>> indexes)
        {
            Exception ex = null;
            bool result = false;

            try
            {
                result = await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted);
            }
            catch(AggregateException ae)
            {
                ex = ae.Flatten().InnerException;
            }

            if(!result)
            {
                await Log.LogErrorAsync(ex.Message);
                return;
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                Identity = identity,
                Indexes = indexes,
                IsEphemeral = true
            };

            string subscriptionUriString = await adapter.SubscribeAsync(resourceUriString, metadata);
        }


    }
}
