using Clients.Security;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Client.Mqtt
{
    class Program
    {
        static string audience = "http://www.skunklab.io/";
        static string issuer = "http://www.skunklab.io/";
        static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static string nameClaimType = "http://www.skunklab.io/name";
        static string roleClaimType = "http://www.skunklab.io/role";
        static string endpoint = "ws://localhost:4163/api/connect";

        static string resourceA = "http://www.skunklab.io/resourcea";
        static string resourceB = "http://www.skunklab.io/resourceb";

        static int channelId;
        static bool abSwitch;
        static string securityToken;

        static IChannel channel;
        static CancellationTokenSource source;

        static void Main(string[] args)
        {
            WriteHeader();
            SelectChannel();
            SelectRole();
            
            securityToken = GetSecurityToken(abSwitch ? "A" : "B");

            string pubResource = abSwitch ? resourceA : resourceB;
            string subResource = abSwitch ? resourceB : resourceA;

            SetChannel();
            channel.OnStateChange += Channel_OnStateChange;

            MqttConfig config = new MqttConfig(null, 180.0, 60.0, 1.5, 1, 100.0);
            config.IdentityClaimType = nameClaimType;

            //create client
            PiraeusMqttClient client = new PiraeusMqttClient(config, channel);

            
            //connect
            ConnectAckCode code = client.ConnectAsync("myId", "JWT", securityToken, 90).Result;
            WriteConnectionResult(code);

            //setup an action to receive subscription data
            client.RegisterTopic(subResource, Subscription);

            //subscribe 
            Task subTask = client.SubscribeAsync(subResource, QualityOfServiceLevelType.AtLeastOnce, Subscription);
            Task.WhenAll(subTask);

            Console.WriteLine("Subscribed to resource '{0}'", subResource);

            bool again = true;

            while(again)
            {
                Console.Write("Send messages (Y/N) ? ");
                again = Console.ReadLine().ToLowerInvariant() == "y";
                if(again)
                {
                    Console.Write("# of messages to send ? ");
                    int numMsgs = Int32.Parse(Console.ReadLine());
                    Console.Write("Enter delay in milliseconds between messages ( <=0 is no delay) ? ");
                    int delay = Int32.Parse(Console.ReadLine());

                    int index = 0;
                    Console.Write("Press enter to start sending ?");
                    Console.ReadKey();

                    while(index < numMsgs)
                    {
                        string message = String.Format("Hello from {0} {1}", pubResource, index);
                        //Pub(pubResource, message, client);
                        Task pt = client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, pubResource, "text/plain", Encoding.UTF8.GetBytes(message));
                        Task.WhenAll(pt);

                        if(delay > 0)
                        {
                            Task delayTask = Task.Delay(delay);
                            Task.WaitAll(delayTask);
                        }
                        Console.WriteLine(index);
                        index++;
                    }

                }
            }

            source.Cancel();
            Console.WriteLine("Completed...press any key to exit.");
            Console.ReadKey();
        }

        static Task Pub(string resource, string message, PiraeusMqttClient client)
        {
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            Task pt = client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, resource, "text/plain", Encoding.UTF8.GetBytes(message));
            tcs.SetResult(null);
            return tcs.Task;
        }

        static void Subscription(string resourceUriString, string contentType, byte[] payload)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Received = {0}", Encoding.UTF8.GetString(payload));
            Console.ResetColor();
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.WriteLine("Channel state = {0}", e.State);
        }

        #region selections
        static void SelectRole()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Select a role (A/B) ? ");
            abSwitch = Console.ReadLine().ToUpperInvariant() == "A";
            Console.ResetColor();
        }

        static void SelectChannel()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Select channel ---");
            Console.WriteLine("(1) Web Socket");
            Console.WriteLine("(2) TCP - Port 1883");
            Console.WriteLine("(3) TCP - Port 8883");
            Console.Write("Enter selection (1/2/3) ? ");
            int id = 0;
            if(Int32.TryParse(Console.ReadLine(), out id))
            {
                if(id > 0 && id < 4)
                {
                    channelId = id;
                }
                else
                {
                    SelectChannel();
                }
            }
            else
            {
                SelectChannel();
            }
        }

        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("--- MQTT Client ---");
            Console.WriteLine("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        #endregion

        static void WriteConnectionResult(ConnectAckCode code)
        {
            Console.ForegroundColor = code == ConnectAckCode.ConnectionAccepted ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("Mqtt connection {0}", code);
            Console.ResetColor();
        }

        static void SetChannel()
        {
            int port = channelId == 2 ? 1883 : 8883;
            source = new CancellationTokenSource();
            channel = channelId == 1 ? ChannelFactory.Create(new Uri(endpoint), securityToken, "mqtt", new WebSocketConfig(), source.Token) : new TcpClientChannel("localhost", port, 2048, source.Token);     //new TcpClientChannel("localhost", port, source.Token); //new TcpClientChannel2("localhost", port, 1024, 2048, source.Token);

        }

        static string GetSecurityToken(string role)
        {
            string roleClaimValue = abSwitch ? "A" : "B";
            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, Guid.NewGuid().ToString()),
                new Claim(roleClaimType, roleClaimValue)
            };

            return ClientToken.CreateJwt(audience, issuer, claims, symmetricKey, 60.0);
        }
    }
}
