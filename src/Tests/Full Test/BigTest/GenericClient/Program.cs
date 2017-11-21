using Piraeus.Clients.Rest;
using SkunkLab.Channels;
using SkunkLab.Channels.Http;
using SkunkLab.Channels.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericClient
{
    class Program
    {
        static string endpoint = "http://localhost:3111/api/connect";
        static string ws_endpoint = "ws://localhost:3111/api/connect";
        static string resource1 = "";
        static string resource2 = "";
        static IChannel channel;
        static void Main(string[] args)
        {
            int channelNo = SelectChannel();
            int protocolNo = SelectProtocol(channelNo);
            string transmitResource = SelectTransmitResource();
            string subscribeResource = SelectTransmitResource();
            string securityToken = null;

            SetChannel(channelNo, protocolNo, endpoint, securityToken, "text/plain", transmitResource, subscribeResource);
            

            //run the client;

        }

        static void RestSender()
        {
            IChannel channel = ChannelFactory.Create("endpoint", "resource", "ct", "token");

            RestClient rc = new RestClient(new Uri(""), "resource", "ct", "token", new List<KeyValuePair<string, string>>(), CancellationToken.None);
            
            HttpObserver observer = new HttpObserver(new Uri("resource"));
            RestClient rc2 = new RestClient(new Uri(""), "token", new Observer[] { observer }, CancellationToken.None);
            rc2.ReceiveAsync();

        }

        static void CreateRestClient(string httpEndpoint, string transmitResource, string)

        static void RunClient(int protocolNo)
        {
            if(protocolNo == 1)
            {
                
            }
            else
            {
                Task openTask = channel.OpenAsync();
                Task.WaitAll(openTask);

                Task receiveTask = channel.ReceiveAsync();
                Task.WhenAll(receiveTask);
            }
        }

        public static void SetChannel(int channelNo, int protocolNo, string endpoint, string securityToken, string contentType, string resourceOut, string resourceIn)
        {

            if(channelNo == 2)
            {
                channel = new WebSocketClientChannel(new Uri(endpoint), securityToken, protocolNo == 2 ? "coapv1" : "mqtt",
                                        new WebSocketConfig(), CancellationToken.None);

            }
        }

        static string SelectTransmitResource()
        {
            Console.WriteLine("Select resource to transmit messages ---");
            Console.WriteLine("(1) {0}", resource1);
            Console.WriteLine("(2) {0}", resource2);
            Console.Write("Select transmit resource no ? ");
            int val = 0;
            if(Int32.TryParse(Console.ReadLine(), out val))
            {
                if(val > 0 && val < 3)
                {
                    return val == 1 ? resource1 : resource2;
                }
                else
                {
                    return SelectTransmitResource();
                }
            }
            else
            {
                return SelectTransmitResource();
            }
        }

        static string SelectSubscribeResource()
        {
            Console.WriteLine("Select resource to subscribe messages ---");
            Console.WriteLine("(1) {0}", resource1);
            Console.WriteLine("(2) {0}", resource2);
            Console.Write("Select transmit resource no ? ");
            int val = 0;
            if (Int32.TryParse(Console.ReadLine(), out val))
            {
                if (val > 0 && val < 3)
                {
                    return val == 1 ? resource1 : resource2;
                }
                else
                {
                    return SelectSubscribeResource();
                }
            }
            else
            {
                return SelectSubscribeResource();
            }
        }

        static int SelectProtocol(int channelNo)
        {
            Console.WriteLine("Select Protocol ---");
            int index = 0;
            index++;

            switch(channelNo)
            {
                case 1:
                    Console.WriteLine("(1) REST");
                    break;
                case 2:
                    Console.WriteLine("(2) CoAP");
                    Console.WriteLine("(3) MQTT");
                    break;
                case 3:
                    Console.WriteLine("(2) CoAP");
                    Console.WriteLine("(3) MQTT");
                    break;
                default:
                    Console.WriteLine("Invalid channel no!");
                    return -1;
            }

            Console.Write("Enter selection number ? ");
            int val = 0;
            if(Int32.TryParse(Console.ReadLine(), out val))
            {
                if(val > 0 && val < 4)
                {
                    return val;
                }
                else
                {
                    return SelectProtocol(channelNo);
                }
            }
            else
            {
                return SelectProtocol(channelNo);
            }

        }

        static int SelectChannel()
        {
            Console.WriteLine("Select Channel ---");
            Console.WriteLine("(1) HTTP");
            Console.WriteLine("(2) Web Socket");
            Console.WriteLine("(3) TCP");
            Console.Write("Enter selection # ? ");
            int value = 0;
            if(Int32.TryParse(Console.ReadLine(), out value))
            {
                if (value > 0 && value < 4)
                    return value;
                else
                    return SelectChannel();
            }
            else
            {
                return SelectChannel();
            }
        }
    }
}
