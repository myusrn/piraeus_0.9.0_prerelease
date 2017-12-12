using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {  
        public CoapProtocolAdapter(CoapConfig config, IChannel channel)
        {            
            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnSent += Channel_OnSent;
            Channel.OnStateChange += Channel_OnStateChange;
            session = new CoapSession(config);            
        }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;
        private CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private bool disposedValue;
        


        public override IChannel Channel { get; set; }


        public override void Init()
        {
            dispatcher = new CoapRequestDispatcher(session, Channel);
            Task task = Channel.OpenAsync();
            Task.WaitAll(task);
        }

     


        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dispatcher.Dispose();
                    session.Dispose();
                    Channel.Dispose();

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

        #region Channel Events
        

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            //opened = true;
            Task task = Log.LogInfoAsync("Channel {0} opened.", e.ChannelId);
            Task.WhenAll(task);

            session.IsAuthenticated = Channel.IsAuthenticated;

            try
            {
                if (!Channel.IsAuthenticated)
                {
                    CoapMessage msg = CoapMessage.DecodeMessage(e.Message);
                    CoapUri coapUri = new CoapUri(msg.ResourceUri.ToString());
                    session.IsAuthenticated = session.Authenticate(coapUri.TokenType, coapUri.SecurityToken);
                }
            }
            catch (Exception ex)
            {
                Task t = Log.LogErrorAsync("Channel {0} not authenticated. Exception {1}", Channel.Id, ex.Message);
                Task.WhenAll(t);
            }

            if (!session.IsAuthenticated)
            {
                //close the channel
                Task logTask = Log.LogErrorAsync("Channel {0} not authenticated. Closing channel.", Channel.Id);
                Task.WhenAll(logTask);

                Task closeTask = Channel.CloseAsync();
                Task.WaitAll(closeTask);
            }

        }
        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            CoapMessage message = CoapMessage.DecodeMessage(e.Message);            

            OnObserve?.Invoke(this, new ChannelObserverEventArgs(message.ResourceUri.ToString(), MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload));
            
            CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);

            Task t = Task.Factory.StartNew(async () =>
            {
                CoapMessage msg = await handler.ProcessAsync();
                if(msg != null)
                {
                    await Channel.SendAsync(msg.Encode());
                }
            });

            Task.WaitAll(t);

            //Task.WhenAll(t);
            //Task<CoapMessage> task = handler.ProcessAsync();
            //Task.WhenAll<CoapMessage>(task);
            //CoapMessage response = task.Result;

            //if (response != null)
            //{
            //    Task sendTask = Channel.SendAsync(response.Encode());
            //    Task.WhenAll(sendTask);
            //}
        }
        
        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));

            Task task = Log.LogErrorAsync("Channel ID {0} with error {1}", Channel.Id, e.Error.Message);
            Task.WhenAll(task);

            Task closeTask = Channel.CloseAsync();
            Task.WhenAll(closeTask);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
            //Channel.Dispose();

            Task task = Log.LogInfoAsync("Channel {0} closing.", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} state {1}", Channel.Id, e.State);
            Task.WhenAll(task);
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} send message", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} retrying message", Channel.Id);
            Task.WhenAll(task);
        }
        #endregion
    }
}
