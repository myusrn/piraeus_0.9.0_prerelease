using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels.WebSocket;
using System.Threading;
using Piraeus.Clients.Coap;
using SkunkLab.Protocols.Coap;

namespace TestWebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press key to start client...");
            Console.ReadKey();

            CancellationTokenSource source = new CancellationTokenSource();
            WebSocketConfig config = new WebSocketConfig();
            WebSocketChannel channel = new WebSocketClientChannel(new Uri("ws://localhost:35736/api/connect"), config, source.Token);
            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;
            channel.OnStateChange += Channel_OnStateChange;



            Task t = channel.OpenAsync();
            Task.WaitAll(t);

            Task task = channel.ReceiveAsync();
            Task.WhenAll(task);

            Console.WriteLine("Press key to send");
            Console.ReadKey();






            Random ran = new Random();
            int length = 1;
            int index = 0;
            while(index < 100)
            {
                byte[] buffer = new byte[length];
                ran.NextBytes(buffer);
                length++;

                CoapConfig cc = new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.Observe | CoapConfigOptions.NoResponse);
                PiraeusCoapClient pcc = new PiraeusCoapClient(cc, channel);
                TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();



                Task pubTask = pcc.PublishAsync("http://www.skunklab.io/resourceb", "application/octet-stream", buffer, false, null);
                tcs.SetResult(pubTask);
                Console.WriteLine("Sent {0} with {1}", index, length);
                

                //Task t1 = channel.SendAsync(buffer);
                //Task.WhenAll(t1);
            //channel.Send(Encoding.UTF8.GetBytes("hi"));
                index++;
            }
            

            Console.WriteLine("sent");
            Console.ReadKey();




        }

        private static void Channel_OnStateChange(object sender, SkunkLab.Channels.ChannelStateEventArgs e)
        {
            Console.WriteLine("State Change = {0}", e.State);
        }

        private static void Channel_OnClose(object sender, SkunkLab.Channels.ChannelCloseEventArgs e)
        {
            Console.WriteLine("Client closed");
        }

        private static void Channel_OnError(object sender, SkunkLab.Channels.ChannelErrorEventArgs e)
        {
            Console.WriteLine("Client error {0}", e.Error.Message);
        }

        private static void Channel_OnReceive(object sender, SkunkLab.Channels.ChannelReceivedEventArgs e)
        {
            Console.WriteLine("Client received {0}", e.Message.Length);
        }

        private static void Channel_OnOpen(object sender, SkunkLab.Channels.ChannelOpenEventArgs e)
        {
            Console.WriteLine("Client open");
        }
    }
}
