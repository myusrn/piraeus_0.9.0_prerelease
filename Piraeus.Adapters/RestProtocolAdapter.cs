using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Channels;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Identity;

namespace Piraeus.Adapters
{
    public class RestProtocolAdapter : ProtocolAdapter
    {
        public RestProtocolAdapter(IChannel channel, IAuthenticator authenticator, string identityClaimType, List<KeyValuePair<string, string>> indexes)
        {
            authn = authenticator;
            this.identityClaimType = identityClaimType;
            this.indexes = indexes;
            Channel = channel;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
        }
        

        public override IChannel Channel { get; set; }
        private string identityClaimType;
        private IAuthenticator authn;
        private List<KeyValuePair<string, string>> indexes;
        private OrleansAdapter adapter;
        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        public event EventHandler<byte[]> OnMessage;
        private bool disposedValue;

        public override void Init()
        {
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            byte[] payload = ProtocolTransition.ConvertToHttp(e.Message);
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(e.Message.ResourceUri, e.Message.ContentType, e.Message.Message));
        }

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            MessageUri uri = new MessageUri(args.Message);

            if(!Channel.IsAuthenticated)  //requires channel authentication
            {
                //use security token validator
                SkunkLab.Security.Tokens.SecurityTokenType tokenType = (SkunkLab.Security.Tokens.SecurityTokenType)Enum.Parse(typeof(SkunkLab.Security.Tokens.SecurityTokenType), uri.TokenType, true);                
                if(!authn.Authenticate(tokenType, uri.SecurityToken))
                {
                    Task task = Channel.CloseAsync();
                    Task.WhenAll(task);
                    return;
                }                
            }

            IdentityDecoder decoder = new IdentityDecoder(identityClaimType, indexes);

            HttpRequestMessage request = (HttpRequestMessage)args.Message;

            if(request.Method == HttpMethod.Get)
            {
                foreach(var item in uri.Subscriptions)
                {
                    Task task = SubscribeAsync(item, decoder.Id, decoder.Indexes);
                    Task.WhenAll(task);
                }
            }else if(request.Method == HttpMethod.Post)
            {
                EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, request.Content.ReadAsByteArrayAsync().Result);
                Task task = PublishAsync(message, new List<KeyValuePair<string, string>>(uri.Indexes));
                Task.WhenAll(task);
            }     
            else
            {
                Task task = Channel.CloseAsync();
                Task.WhenAll(task);
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {            
        }


        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

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
            if(!await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted))
            {
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
