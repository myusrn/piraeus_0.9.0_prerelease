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

namespace Piraeus.Adapters
{
    public class RestProtocolAdapter : ProtocolAdapter
    {
        public RestProtocolAdapter(IChannel channel)
        {
            Channel = channel;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
        }
        

        public override IChannel Channel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override event ProtocolAdapterErrorHandler OnError;
        public override event ProtocolAdapterCloseHandler OnClose;
        private bool disposedValue;

        public override void Init()
        {
            throw new NotImplementedException();
        }

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            if(!Channel.IsAuthenticated)  //requires channel authentication
            {
                //close the channel.
            }

            HttpRequestMessage request = (HttpRequestMessage)args.Message;
            MessageUri uri = new MessageUri(request);
            IResource resource = GraphManager.GetResource(uri.Resource);
            ResourceMetadata metadata = resource.GetMetadataAsync().GetAwaiter().GetResult();

            if(metadata != null)
            {
                IAccessControl accessControl = GraphManager.GetAccessControl(metadata.PublishPolicyUriString);
                if(accessControl != null && accessControl.IsAuthorizedAsync(Thread.CurrentPrincipal.Identity as ClaimsIdentity).GetAwaiter().GetResult())
                {
                    EventMessage msg = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
                    msg.Audit = metadata.Audit;
                    Task task = null;
                    if(uri.Indexes != null && uri.Indexes.Count() > 0)
                    {
                        task = resource.PublishAsync(msg, new List<KeyValuePair<string,string>>(uri.Indexes));
                    }
                    else
                    {
                        task = resource.PublishAsync(msg);
                    }

                    Task.WhenAll(task);
                }
            }


            //get the resource;
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


    }
}
