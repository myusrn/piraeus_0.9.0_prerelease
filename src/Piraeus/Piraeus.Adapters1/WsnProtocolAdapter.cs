using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Piraeus.Configuration.Settings;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Security.Identity;

namespace Piraeus.Adapters
{
    public class WsnProtocolAdapter : ProtocolAdapter
    {
        public WsnProtocolAdapter(PiraeusConfig config, IChannel channel)
        {
            this.config = config;
            Channel = channel;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnError += Channel_OnError;
            Channel.OnClose += Channel_OnClose;
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
        }

        

        public override IChannel Channel { get; set; }

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

        private PiraeusConfig config;
        private OrleansAdapter adapter;
        private bool disposedValue;
        private string resourceUriString;
        private string contentType;
        private IdentityDecoder decoder;
        private List<KeyValuePair<string, string>> indexes;
        
        public override void Init()
        {

        } 

        #region Channel Events
        
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            if(!Channel.IsAuthenticated)
            {
                Task logTask = Log.LogErrorAsync("Channel {0} not authenticated.", Channel.Id);
                Task.WhenAll(logTask);

                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated.")));
                Task task = Channel.CloseAsync();
                Task.WhenAll(task);
            }

            decoder = new IdentityDecoder(config.Identity.Client.IdentityClaimType, config.Identity.Client.Indexes);

            MessageUri uri = new MessageUri(e.Message);
            resourceUriString = uri.Resource;
            contentType = uri.ContentType;

            if(uri.Subscriptions != null)
            {
                foreach(string subscription in uri.Subscriptions)
                {
                    Task subTask = SubscriptionAsync(subscription);
                    Task.WhenAll(subTask);
                }
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Task task = PublishAsync(e.Message);
            Task.WhenAll(task);
        }
        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Task task = Log.LogErrorAsync("Channel {0} error with {1}", Channel.Id, e.Error.Message);
            Task.WhenAll(task);

            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }
        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} is closing.", Channel.Id);
            Task.WhenAll(task);

            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
        }


        #endregion

        private async Task PublishAsync(byte[] message)
        {
            EventMessage eventMessage = new EventMessage(contentType, resourceUriString, ProtocolType.WSN, message);
            await adapter.PublishAsync(eventMessage, indexes);
        }


        private async Task SubscriptionAsync(string resourceUriString)
        {
            bool result = false;
            try
            {
                result = await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted);
            }
            catch(AggregateException ae)
            {
                await Log.LogErrorAsync(ae.Flatten().InnerException.Message);
            }

            if (result)
            {
                SubscriptionMetadata metadata = new SubscriptionMetadata();
                metadata.IsEphemeral = true;
                metadata.Identity = decoder.Id;
                metadata.Indexes = decoder.Indexes;

                await adapter.SubscribeAsync(resourceUriString, metadata);
            }
            else
            {
                await Log.LogWarningAsync("Cannot subscribe requested resource {0}", resourceUriString);
            }
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            Task task = Channel.SendAsync(e.Message.Message);
            Task.WhenAll(task);

            OnObserve?.Invoke(this, new ChannelObserverEventArgs(e.Message.ResourceUri, e.Message.ContentType, e.Message.Message));
        }

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
    }
}
