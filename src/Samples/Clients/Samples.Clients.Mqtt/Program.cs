using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Clients.Mqtt
{
    class Program
    {
        //static string audience = "http://www.skunklab.io/";
        //static string issuer = "http://www.skunklab.io/";
        //static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        //static string nameClaimType = "http://www.skunklab.io/name";
        //static string roleClaimType = "http://www.skunklab.io/role";
        //static string endpoint = "ws://localhost:4163/api/connect";

        //private static CancellationTokenSource source;
        //private static bool abSwitch;
        //private static IChannel channel; //communuications channel
        //static string resourceA = "http://www.skunklab.io/resourcea";
        //static string resourceB = "http://www.skunklab.io/resourceb";

        static string audience = "http://www.skunklab.io/";
        static string issuer = "http://www.skunklab.io/";
        static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static string nameClaimType = "http://www.skunklab.io/name";
        static string roleClaimType = "http://www.skunklab.io/role";
        static string resourceA = "http://www.skunklab.io/resourcea";
        static string resourceB = "http://www.skunklab.io/resourceb";

        static string endpoint = "ws://localhost:4163/api/connect";

        static int channelNo;
        static int index;
        static IChannel channel;
        static CancellationTokenSource source;
        static string contentType = "text/plain";
        static string publishResource;
        static string observeResource;
        static string role;
        static string clientName;


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

            //source = new CancellationTokenSource();

            //Console.WriteLine("MQTT Client press any key to continue.");
            //Console.ReadKey();

            //Console.Write("Select a role (A/B) ? ");
            //abSwitch = Console.ReadLine().ToUpperInvariant() == "A";

            //string token = GetSecurityToken(abSwitch ? "A" : "B");

            //channel = ChannelFactory.Create(new Uri(endpoint), token, "mqtt", new WebSocketConfig(), source.Token);
            //channel.OnStateChange += Channel_OnStateChange1;
            //channel.OnClose += Channel_OnClose;
            //channel.OnError += Channel_OnError;


            //string pubResource = abSwitch ? resourceA : resourceB;
            //string subResource = abSwitch ? resourceB : resourceA;

            MqttConfig config = new MqttConfig();
            PiraeusMqttClient client = new PiraeusMqttClient(config, channel);
            client.RegisterTopic(observeResource, ObserveResource);
            ConnectAckCode code = client.ConnectAsync("sessionId", "JWT", securityToken, 90).Result;
            Console.WriteLine("Connected with Ack Code {0}", code);

            try
            {
                Task subTask = client.SubscribeAsync(observeResource, QualityOfServiceLevelType.AtLeastOnce, ObserveResource);
                Task.WaitAll(subTask);

                Console.WriteLine("Subscribed to {0}", observeResource);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Observe failed.");
                Console.WriteLine(ex.InnerException.Message);
                goto endsample;
            }

            SendMessages(client);

            source.Cancel();

            endsample:
            Console.WriteLine("client is closed...");
            Console.ReadKey();            
        }

        static void SendMessages(PiraeusMqttClient client)
        {
            Console.WriteLine();
            Console.Write("Send messages (Y/N) ? ");
            bool sending = Console.ReadLine().ToLowerInvariant() == "y";

            if (sending)
            {
                Console.Write("Enter number of messages to send ? ");
                int num = Int32.Parse(Console.ReadLine());

                Console.WriteLine("Enter delay between messages in milliseconds ? ");
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

                SendMessages(client);
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
            Console.Write("Enter unique client name ? ");
            clientName = Console.ReadLine();

            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, clientName),
                new Claim(roleClaimType, role)
            };

            return CreateJwt(audience, issuer, claims, symmetricKey, 60.0);
        }

        static void SetResources()
        {
            publishResource = role == "A" ? resourceA : resourceB;
            observeResource = role == "A" ? resourceB : resourceA;
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
            if (num == 1)
            {
                Console.Write("Enter Web Socket URL or Enter for default ? ");
                string url = Console.ReadLine();
                url = String.IsNullOrEmpty(url) ? endpoint : url;
                return ChannelFactory.Create(new Uri(url), securityToken, "mqtt", new WebSocketConfig(), source.Token);                
            }
            else if (num == 2)
            {
                Console.Write("Enter TCP remote hostname or Enter for default ? ");
                string hostname = Console.ReadLine();
                hostname = String.IsNullOrEmpty(hostname) ? "localhost" : hostname;
                return ChannelFactory.Create(true, hostname, 8883, 1024, 2048, source.Token);
            }
            else if (num == 3)
            {
                Console.Write("Enter UDP remote hostname or Enter for default ? ");
                string hostname = Console.ReadLine();
                hostname = String.IsNullOrEmpty(hostname) ? "localhost" : hostname;
                Console.Write("Enter UDP port for this client to use ? ");
                int port = Int32.Parse(Console.ReadLine());
                return ChannelFactory.Create(port, hostname, 5883, source.Token);
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
                Console.WriteLine(Encoding.UTF8.GetString(payload));
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
