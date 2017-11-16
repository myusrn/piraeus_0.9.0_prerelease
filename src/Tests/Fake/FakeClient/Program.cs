using Piraeus.Adapters;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CoapProtocolAdapter adapter = new CoapProtocolAdapter(new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe), null);
        }
    }

    public class FakeChannel : IChannel
    {
      
        public bool IsConnected => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public int Port => throw new NotImplementedException();

        public ChannelState State => throw new NotImplementedException();

        public bool IsEncrypted { get { return true; } }

        public bool IsAuthenticated { get { return true; } }

        public event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public event EventHandler<ChannelCloseEventArgs> OnClose;
        public event EventHandler<ChannelOpenEventArgs> OnOpen;
        public event EventHandler<ChannelErrorEventArgs> OnError;
        public event EventHandler<ChannelStateEventArgs> OnStateChange;
        public event EventHandler<ChannelRetryEventArgs> OnRetry;
        public event EventHandler<ChannelSentEventArgs> OnSent;
        public event EventHandler<ChannelObserverEventArgs> OnObserve;

        public async Task AddMessageAsync(byte[] message)
        {
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
        }

        public async Task ReceiveAsync()
        {
        }

        public async Task SendAsync(byte[] message)
        {
            CoapMessage cm = CoapMessage.DecodeMessage(message);
            if(cm.Payload == null)
            {
                Console.WriteLine("Code == {0}", cm.Code);
            }
            else
            {
                Console.WriteLine("Code == {0} + Message == {1}", cm.Code, Encoding.UTF8.GetString(cm.Payload));
            }
        }
    }
}
