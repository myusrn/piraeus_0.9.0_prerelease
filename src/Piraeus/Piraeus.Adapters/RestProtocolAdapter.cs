using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
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
            Channel = channel;
        }

        public override IChannel Channel { get; set; }

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;


        private PiraeusConfig config;
        private OrleansAdapter adapter;
        private bool disposedValue;

        

        public override void Init()
        {
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            
            Task task = Channel.OpenAsync();
            Task.WhenAll(task);
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            if (!Channel.IsAuthenticated)  //requires channel authentication
            {
                Task logTask = Log.LogErrorAsync("Channel {0} not authenticated.", Channel.Id);
                Task.WhenAll(logTask);

                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated.")));
                Task ctask = Channel.CloseAsync();
                Task.WhenAll(ctask);
                return;
            }

            if (e.Message.Method != HttpMethod.Post && e.Message.Method != HttpMethod.Get)
            {
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Rest protocol adapter requires GET or POST only.")));
            }

            MessageUri uri = new MessageUri(e.Message);
            IdentityDecoder decoder = new IdentityDecoder(config.Identity.Client.IdentityClaimType, config.Identity.Client.Indexes);
            HttpRequestMessage request = (HttpRequestMessage)e.Message;

            if (request.Method == HttpMethod.Get)
            {
                foreach (var item in uri.Subscriptions)
                {
                    Task t = Task.Factory.StartNew(async () =>
                    {
                        await SubscribeAsync(item, decoder.Id, decoder.Indexes);
                    });

                    Task.WhenAll(t);
                }
            }

            if (request.Method == HttpMethod.Post)
            {
                byte[] buffer = request.Content.ReadAsByteArrayAsync().Result;
                Task t = Task.Factory.StartNew(async () =>
                {
                    EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, buffer);
                    List<KeyValuePair<string, string>> indexList = uri.Indexes == null ? null : new List<KeyValuePair<string, string>>(uri.Indexes);

                    await PublishAsync(decoder.Id, message, indexList);
                });

                Task.WhenAll(t);

                //EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, request.Content.ReadAsByteArrayAsync().Result);
                //List<KeyValuePair<string, string>> indexList = uri.Indexes == null ? null : new List<KeyValuePair<string, string>>(uri.Indexes);

                //var tcs = new TaskCompletionSource<Task>();
                //Task t = PublishAsync(decoder.Id, message, indexList);
                //tcs.SetResult(t);

                //Task task = PublishAsync(decoder.Id, message, indexList);
                //Task.WhenAll(task);
                //Task final = Channel.CloseAsync();
                //Task.WhenAll(final);
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
        }

        

        

        #region Adapter event
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            byte[] payload = ProtocolTransition.ConvertToHttp(e.Message);
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(e.Message.ResourceUri, e.Message.ContentType, payload));
        }

        #endregion

        #region Dispose

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


        #region private methods

        private async Task SubscribeAsync(string resourceUriString, string identity, List<KeyValuePair<string, string>> indexes)
        {          
            if(await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted))
            {
                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    Identity = identity,
                    Indexes = indexes,
                    IsEphemeral = true
                };

                string subscriptionUriString = await adapter.SubscribeAsync(resourceUriString, metadata);
                await Log.LogInfoAsync("Identity {0} subscribed to resource {1} with subscription URI {2}", identity, resourceUriString, subscriptionUriString);
            }
            else
            {
                await Log.LogErrorAsync("REST protocol cannot subscribe identity {0} to resource {1}", identity, resourceUriString);
            }
        }

        private async Task PublishAsync(string identity, EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            if (await adapter.CanPublishAsync(message.ResourceUri, Channel.IsEncrypted))
            {
                await adapter.PublishAsync(message, indexes);
            }
            else
            {
                await Log.LogErrorAsync("Identity {0} cannot publish to resource {1}", identity, message.ResourceUri);
            }            
        }
        #endregion
    }
}
