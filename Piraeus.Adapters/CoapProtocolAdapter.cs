using System;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace Piraeus.Adapters
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {
        public CoapProtocolAdapter(IChannel channel, CoapConfig config = null)
        {
            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnSent += Channel_OnSent;
            Channel.OnStateChange += Channel_OnStateChange;

            if (config != null)
            {
                session = new CoapSession(config);
            }
            else
            {
                session = new CoapSession(ProtocolAdapter.CoapConfig);
            }
        }

       

        public override IChannel Channel { get; set; }

        public override event ProtocolAdapterErrorHandler OnError;
        public override event ProtocolAdapterCloseHandler OnClose;
        private CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private bool disposedValue;

        public override void Init()
        {
            dispatcher = new CoapRequestDispatcher(session,Channel);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dispatcher.Dispose();
                    session.Dispose();
                    
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

        #region Channel events
        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} state {1}", Channel.Id, args.State);
            Task.WhenAll(task);
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} send message", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} retrying message", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            CoapMessage message = CoapMessage.DecodeMessage(args.Message);
            CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);
            Task<CoapMessage> task = handler.ProcessAsync();
            Task.WhenAll<CoapMessage>(task);
            CoapMessage response = task.Result;

            if(response != null)
            {
                Channel.SendAsync(response.Encode());
            }
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;

            try
            {
                if (!Channel.IsAuthenticated)
                {
                    CoapMessage msg = CoapMessage.DecodeMessage(args.Message);
                    CoapUri coapUri = new CoapUri(msg.ResourceUri.ToString());
                    session.IsAuthenticated = session.Authenticate(coapUri.TokenType, coapUri.SecurityToken);
                }
            }
            catch(Exception ex)
            {
                Task task = Log.LogErrorAsync("Channel {0} not authenticated.", Channel.Id);
                Task.WhenAll(task);
            }

            if(!session.IsAuthenticated)
            {
                //close the channel
                Task task = Channel.CloseAsync();
                Task.WaitAll(task);
            }            
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            Task task = Log.LogErrorAsync("Channel ID {0} with error {1}", Channel.Id, args.Error.Message);
            Task.WhenAll(task);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            Task task = Log.LogInfoAsync("Channel {0} closing.", Channel.Id);
            Task.WhenAll(task);
        }

        #endregion


    }
}
