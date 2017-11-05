using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Utilities;
using SkunkLab.Channels;

namespace Piraeus.Adapters
{
    public class HttpPublishProtocolAdapter : ProtocolAdapter
    {
        public HttpPublishProtocolAdapter(IChannel channel)
        {
            adapter = new OrleansAdapter();
            Channel = channel;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
        }

        public override IChannel Channel { get; set; }
        private OrleansAdapter adapter;
        private bool disposedValue;

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

        private MessageUri uri;

        public override void Init()
        {
            Task openTask = Channel.OpenAsync();
            Task.WhenAll(openTask);
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            if (!Channel.IsAuthenticated)
            {
                Task closeTask = Channel.CloseAsync();
                Task.WhenAll(closeTask);
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated.")));
                return;
            }

            //read the resoure uri
            uri = new MessageUri(e.Message);

            //get the payload
            Task<byte[]> task = e.Message.ReadAsByteArrayAsync();
            Task.WhenAll<byte[]>(task);

            byte[] payload = task.Result;

            Task addTask = Channel.AddMessageAsync(payload);
            Task.WhenAll(addTask);
        }


        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, e.Message);
            Task task = PublishAsync(uri.Resource, message, new List<KeyValuePair<string, string>>(uri.Indexes));
            Task.WhenAll(task);
        }

        private async Task PublishAsync(string resourceUriString, EventMessage message, List<KeyValuePair<string,string>> indexes = null)
        {
            if(await adapter.CanPublishAsync(resourceUriString, Channel.IsEncrypted))
            {
                await adapter.PublishAsync(message, indexes);
            }
            await Channel.CloseAsync();
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected void Disposing(bool disposing)
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
    }
}
