using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Channels.Http;
using SkunkLab.Channels.Tcp;
using SkunkLab.Channels.Udp;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace CoapServer
{
    class Program
    {
        static IChannel channel;
        static CancellationTokenSource cts;
        static CoapSession session;
        static RequestDispatcher dispatcher;
        static void Main(string[] args)
        {
            cts = new CancellationTokenSource();
            CoapConfig config = new CoapConfig("www.example.org", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe);
            session = new CoapSession(config);
            TcpServer ts = new TcpServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5684), cts.Token);
            Task runTask = ts.RunAsync();
            Task.WhenAll(runTask);

            dispatcher = new RequestDispatcher();
           
            WriteHeader();

            TcpServer tcp = new TcpServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"),5684), cts.Token);
            Task task = tcp.RunAsync();
            Task.WhenAll(task);

            int channelNo = SelectChannel();
            SetChannel(channelNo);
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError;
            channel.OnRetry += Channel_OnRetry;
            channel.OnSent += Channel_OnSent;
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnReceive += Channel_OnReceive;
            channel.OnOpen += Channel_OnOpen;

            Task task = channel.OpenAsync();
            Task.WhenAll(task);
            
        }

        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel Open");
            Console.ResetColor();
        }

        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel Receiving");
            Console.ResetColor();

            CoapMessage msg = CoapMessage.DecodeMessage(args.Message);
            CoapMessageHandler handler = CoapMessageHandler.Create(session, msg, dispatcher);
            Task<CoapMessage> task = handler.ProcessAsync();
            Task<CoapMessage>.WhenAll(task);
            CoapMessage response = task.Result;

            if(response != null)
            {
                Task sendTask = channel.SendAsync(response.Encode());
                Task.WhenAll(sendTask);
            }
            
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel State {0}", args.State);
            Console.ResetColor();
        }

        private static void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Message Sent {0}", args.MessageId);
            Console.ResetColor();
        }

        private static void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Channel Error {0}", args.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel Closed");
            Console.ResetColor();
        }

        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("------------------------------------");
            Console.WriteLine("    Coap Protocol Server Test");
            Console.WriteLine("------------------------------------");
            Console.ResetColor();
        }

        static int SelectChannel()
        {
            Console.WriteLine();
            Console.WriteLine("Channel Selector");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("(1) TCP");
            Console.WriteLine("(2) UDP");
            Console.WriteLine("(3) HTTP");
            Console.WriteLine("(4) Web Socket");
            Console.Write("Enter Channel Number ? ");
            int channelNo = 0;
            if(!Int32.TryParse(Console.ReadLine(), out channelNo))
            {
                SelectChannel();
            }
            else if(channelNo > 0 && channelNo < 5)
            {
                return channelNo;
            }
            else
            {
                SelectChannel();
            }

            return channelNo;
        }

        private static void SetChannel(int channelNo)
        {
            if(channelNo == 1)
            {
                TcpClient tcpclient = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 22222));
                
                channel = new TcpServerChannel(tcpclient, cts.Token);
            }
            else if(channelNo == 2)
            {
                //channel = new UdpServerChannel()
            }
            else if(channelNo == 3)
            {
                //channel = new HttpServerChannel();
            }
            else if(channelNo == 4)
            {
                //channel = new WebSocketServerChannel();
            }
        }
    }
}
