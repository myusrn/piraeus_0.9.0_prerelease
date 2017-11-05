using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace SkunkLab.Core.Adapters
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
        private bool authenticated;
        private CoapSession session;
       

        public override void Init()
        {
           
        }



        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        #region Channel events
        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
           // SkunkLab.Protocols.Coap.ICoapRequestDispatch dispath;
           //CoapMessageHandler handler = CoapMessageHandler.Create(session, )
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;           
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


    }
}
