using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FakeClient
{
    public class FakeChannel : IChannel
    {
        public int ProtocolNo { get; set; }
        public bool IsConnected { get; set; }

        public string Id { get; set; }

        public int Port { get; set; }

        public ChannelState State { get; set; }

        public bool IsEncrypted { get { return true; } }

        public bool IsAuthenticated { get { return true; } }

        public event System.EventHandler<ChannelReceivedEventArgs> OnReceive;
        public event System.EventHandler<ChannelCloseEventArgs> OnClose;
        public event System.EventHandler<ChannelOpenEventArgs> OnOpen;
        public event System.EventHandler<ChannelErrorEventArgs> OnError;
        public event System.EventHandler<ChannelStateEventArgs> OnStateChange;
        public event System.EventHandler<ChannelRetryEventArgs> OnRetry;
        public event System.EventHandler<ChannelSentEventArgs> OnSent;
        public event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        public async Task AddMessageAsync(byte[] message)
        {
            //CoapMessage cm = CoapMessage.DecodeMessage(message);
            //Console.WriteLine("Request {0} {1}", cm.Code, Convert.ToBase64String(cm.Token));
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs("ABC", message));
        }

        public async Task CloseAsync()
        {
        }

        public void Dispose()
        {
        }

        public async Task OpenAsync()
        {
            OnOpen?.Invoke(this, new ChannelOpenEventArgs("ABC", null));
        }

        public async Task ReceiveAsync()
        {
        }

        public async Task SendAsync(byte[] message)
        {
            if (ProtocolNo == 1)
            {
                CoapMessage cm = CoapMessage.DecodeMessage(message);
                if (cm.Payload == null)
                {
                    Console.WriteLine("Code == {0} {1}", cm.Code, Convert.ToBase64String(cm.Token));
                }
                else
                {
                    Console.WriteLine("Code == {0} + Message == {1} {2}", cm.Code, Encoding.UTF8.GetString(cm.Payload), Convert.ToBase64String(cm.Token));
                }
            }
            else
            {
                SkunkLab.Protocols.Mqtt.MqttMessage mm = SkunkLab.Protocols.Mqtt.MqttMessage.DecodeMessage(message);
                if(mm.Payload == null)
                {
                    Console.WriteLine("{0} {1}", mm.MessageId, mm.MessageType);
                }
                else
                {
                    Console.WriteLine("{0} {1} {2}", mm.MessageId, mm.MessageType, Encoding.UTF8.GetString(mm.Payload));
                }
            }
        }
    }
}
