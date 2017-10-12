using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Security;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;

namespace SkunkLab.Core.Adapters
{
    public class MqttProtocolAdapter : ProtocolAdapter
    {
        public MqttProtocolAdapter(IAuthenticator authenticator, IChannel channel)
        {
            session = new MqttSession(new MqttConfig());
            this.authenticator = authenticator;
            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnSent += Channel_OnSent;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnStateChange += Channel_OnStateChange;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;

        }

        private IAuthenticator authenticator;
        public override event ProtocolAdapterCloseHandler OnClose;
        public override event ProtocolAdapterErrorHandler OnError;
        private MqttSession session;


        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
            MqttMessageHandler handler = MqttMessageHandler.Create(session, msg);
            Task<MqttMessage> task = handler.ProcessAsync();
            Task<MqttMessage>.WhenAll(task);
            MqttMessage response = task.Result;            
           
            if(response != null)
            {
                Task sendTask = Channel.SendAsync(response.Encode());
                Task.WhenAll(sendTask);
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
            MqttMessageHandler handler = MqttMessageHandler.Create(session, msg);
            Task<MqttMessage> task = handler.ProcessAsync();
            Task<MqttMessage>.WhenAll(task);
            MqttMessage response = task.Result;

            if (response != null)
            {
                Task sendTask = Channel.SendAsync(response.Encode());
                Task.WhenAll(sendTask);
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override IChannel Channel { get; set; }

        


        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    
}
