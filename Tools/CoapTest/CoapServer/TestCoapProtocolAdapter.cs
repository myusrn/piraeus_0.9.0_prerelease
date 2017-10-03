using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Core;
using SkunkLab.Core.Adapters;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace CoapServer
{
    public class TestCoapProtocolAdapter : ProtocolAdapter
    {
        public TestCoapProtocolAdapter(IChannel channel, CoapConfig config) 
        {
            session = new CoapSession(config);
            Channel = channel;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnOpen += Channel_OnOpen;
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            if(args.Message != null)
            {
                //must be the initial request                

            }
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            Console.WriteLine("Channel closing");
        }

        private CoapSession session;
        public override IChannel Channel { get; set; }

        public override event ProtocolAdapterErrorHandler OnError;
        public override event ProtocolAdapterCloseHandler OnClose;
        public override void Init()
        {
            
        }

        public override void Dispose()
        {
           
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            CoapMessage message = null;

            try
            {
                message = CoapMessage.DecodeMessage(args.Message);
                CoapMessageHandler handler = CoapMessageHandler.Create(session, message);
                Task<CoapMessage> task = handler.ProcessAsync();
                Task<CoapMessage>.WhenAll(task);
                CoapMessage msg = task.Result;
                if (msg != null)
                {
                    Task t = Channel.SendAsync(msg.Encode());
                    Task.WhenAll(t);
                }
            }
            catch(Exception ex)
            {
                Task task = Log.LogErrorAsync("Receive failed for CoAP protocol adapter {0}", ex.Message);
                Task.WhenAll(task);
            }
        }




    }
}
