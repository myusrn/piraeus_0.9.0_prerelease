using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Clients.Mqtt
{
    class Program
    {
        

        static int channelNo;
        static int index;
        static IChannel channel;
        static CancellationTokenSource source;
        static string publishResource;
        static string observeResource;
        static string role;
        static string clientName;
        static PiraeusMqttClient client;


        static void Main(string[] args)
        {
            source = new CancellationTokenSource();
            WriteHeader();  //descriptive header
            SelectClientRole(); //select a role for the client

            string securityToken = GetSecurityToken();  //get the security token with a unique name
            SetResources(); //setup the resources for pub and observe based on role.

            //Note: Must start the Web gateway and/or TCP/UDP gateway to be able to communicate
            SelectChannel(); //pick a channel for communication 

            channel = GetChannel(channelNo, securityToken);
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;
                      

            MqttConfig config = new MqttConfig();
            client = new PiraeusMqttClient(config, channel);
            client.RegisterTopic(observeResource, ObserveResource);
            ConnectAckCode code = client.ConnectAsync("sessionId", "JWT", securityToken, 90).Result;
            Console.WriteLine("Connected with Ack Code {0}", code);

            try
            {
                Task subTask = client.SubscribeAsync(observeResource, QualityOfServiceLevelType.AtLeastOnce, ObserveResource).ContinueWith(SendMessages);
                Task.WaitAll(subTask);

                Console.WriteLine("Subscribed to {0}", observeResource);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Observe failed.");
                Console.WriteLine(ex.InnerException.Message);
                goto endsample;
            }

            //SendMessages();

            source.Cancel();

            endsample:
            Console.WriteLine("client is closed...");
            Console.ReadKey();            
        }

        static void SendMessages(Task task)
        {
            Console.WriteLine();
            Console.Write("Send messages (Y/N) ? ");
            bool sending = Console.ReadLine().ToLowerInvariant() == "y";

            if (sending)
            {
                Console.Write("Enter number of messages to send ? ");
                int num = Int32.Parse(Console.ReadLine());

                Console.Write("Enter delay between messages in milliseconds ? ");
                int delay = Int32.Parse(Console.ReadLine());

                for (int i = 0; i < num; i++)
                {
                    index++;
                    //send a message to a resource
                    string message = String.Format("{0} sent message {1}", clientName, index);
                    byte[] payload = Encoding.UTF8.GetBytes(message);
                    Task pubTask = client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, publishResource, "text/plain", payload);                    
                    Task.WhenAll(pubTask);

                    if (delay > 0)
                    {
                        Task.Delay(delay).Wait();
                    }
                }

                SendMessages(task);
            }
        }

        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- MQTT Client ---");
            Console.WriteLine("press any key to continue...");
            Console.WriteLine();
            Console.ResetColor();
            Console.ReadKey();
        }

        static void SelectClientRole()
        {
            Console.WriteLine();
            Console.Write("Enter Role for this client (A/B) ? ");
            role = Console.ReadLine().ToUpperInvariant();
            if (role != "A" && role != "B")
                SelectClientRole();
        }

        static string GetSecurityToken()
        {
            string nameClaimType = ConfigurationManager.AppSettings["nameClaimType"];
            string roleClaimType = ConfigurationManager.AppSettings["roleClaimType"];

            Console.Write("Enter unique client name ? ");
            clientName = Console.ReadLine();

            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, clientName),
                new Claim(roleClaimType, role)
            };

            string audience = ConfigurationManager.AppSettings["audience"];
            string issuer = ConfigurationManager.AppSettings["issuer"];
            string symmetricKey = ConfigurationManager.AppSettings["symmetricKey"];

            return CreateJwt(audience, issuer, claims, symmetricKey, 60.0);
        }

        static void SetResources()
        {
            string resource1 = ConfigurationManager.AppSettings["resource1"];
            string resource2 = ConfigurationManager.AppSettings["resource2"];
            publishResource = role == "A" ? resource1 : resource2;
            observeResource = role == "A" ? resource2 : resource1;
        }

        static void SelectChannel()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- Select Channel ---");
            Console.WriteLine("(1) Web Socket");
            Console.WriteLine("(2) TCP");
            Console.WriteLine("(3) UDP");
            Console.Write("Enter selection # ? ");

            int num = 0;
            if (Int32.TryParse(Console.ReadLine(), out num) && num > 0 && num < 4)
            {
                channelNo = num;
            }
            else
            {
                Console.WriteLine("Try again...");
                SelectChannel();
            }

        }

        private static IChannel GetChannel(int num, string securityToken)
        {
            Console.Write("Enter hostname, IP, or Enter for localhost ? ");
            string hostnameOrIP = Console.ReadLine();
            IPAddress address = null;
            bool isIP = IPAddress.TryParse(hostnameOrIP, out address);
            string authority = isIP ? address.ToString() : String.IsNullOrEmpty(hostnameOrIP) ? "localhost" : hostnameOrIP;


            if (num == 1)
            {
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["localhostPort"]);
                string uriString = authority.Contains("localhost") ?
                                    String.Format("ws://{0}:{1}/api/connect", authority, port) :
                                    String.Format("ws://{0}/api/connect", authority);

                return ChannelFactory.Create(new Uri(uriString), securityToken, "mqtt", new WebSocketConfig(), source.Token);

            }
            else if (num == 2)
            {

                IChannel channel = address == null ?
                                     ChannelFactory.Create(true, authority, 8883, 1024, 2048, source.Token) :
                                     ChannelFactory.Create(true, address, 8883, 1024, 2048, source.Token);

                return channel;
            }
            else if (num == 3)
            {
                Console.Write("Enter UDP local port for this client ? ");
                int port = Int32.Parse(Console.ReadLine());

                if (address != null)
                {
                    IPEndPoint endpoint = new IPEndPoint(address, 5883);
                    return ChannelFactory.Create(port, endpoint, source.Token);
                }
                else
                {
                    return ChannelFactory.Create(port, hostnameOrIP, 5883, source.Token);
                }
            }

            return null;
        }


        public static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            return jwt.ToString();
        }

        static void ObserveResource(string resourceUriString, string contentType, byte[] payload)
        {
            if (payload != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Encoding.UTF8.GetString(payload));
                Console.ResetColor();
            }
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel State {0}", e.State);
            Console.ResetColor();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel closed");
            Console.ResetColor();
        }
                
    }
}
