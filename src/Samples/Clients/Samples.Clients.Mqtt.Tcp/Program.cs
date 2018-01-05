using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Clients.Mqtt.Tcp
{
    class Program
    {
        static string audience = "http://www.skunklab.io/";
        static string issuer = "http://www.skunklab.io/";
        static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static string nameClaimType = "http://www.skunklab.io/name";
        static string roleClaimType = "http://www.skunklab.io/role";
        static string endpoint = "ws://localhost:4163/api/connect";

        private static CancellationTokenSource source;
        private static bool abSwitch;
        private static IChannel channel; //communuications channel
        static string resourceA = "http://www.skunklab.io/resourcea";
        static string resourceB = "http://www.skunklab.io/resourceb";

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();

            Console.WriteLine("MQTT Client (TCP) press any key to continue.");
            Console.ReadKey();

            Console.Write("Select a role (A/B) ? ");
            abSwitch = Console.ReadLine().ToUpperInvariant() == "A";

            string token = GetSecurityToken(abSwitch ? "A" : "B");

            string pubResource = abSwitch ? resourceA : resourceB;
            string subResource = abSwitch ? resourceB : resourceA;

            channel = new TcpClientChannel2("localhost", 1883, 1024, 2048, source.Token);

            MqttConfig config = new MqttConfig(null, 180.0, 60.0, 1.5, 1, 100.0);
            config.IdentityClaimType = nameClaimType;

            Piraeus.Clients.Mqtt.PiraeusMqttClient client = new Piraeus.Clients.Mqtt.PiraeusMqttClient(config, channel);
            client.RegisterTopic(subResource, Subscription);
            ConnectAckCode code = client.ConnectAsync("sessionId", "JWT", token, 90).Result;
            Console.WriteLine("Connected with Ack Code {0}", code);

            Task subTask = client.SubscribeAsync(subResource, QualityOfServiceLevelType.AtLeastOnce, Subscription);
            Task.WhenAll(subTask);

            Console.WriteLine("Subscribed to {0}", subResource);
            Console.WriteLine("Waiting for messages...");
            Console.Write("Press a key to send (Y/N) ? ");
            int index = 0;
            bool trySend = Console.ReadLine().ToLowerInvariant() == "y";

            while (trySend)
            {
                while (index < 1)
                {
                    string message = String.Format("Hi from {0} {1}", pubResource, index++);
                    Task pt = client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, pubResource, "text/plain", Encoding.UTF8.GetBytes(message));
                    Task.WhenAll(pt);

                    Console.WriteLine("Sent message to {0}", pubResource);
                }

                index = 0;
                Console.Write("Press a key to send (Y/N) ? ");
                trySend = Console.ReadLine().ToLowerInvariant() == "y";
            }
            Console.ReadKey();
        }

        static void Subscription(string a, string b, byte[] payload)
        {
            Console.WriteLine("Received = {0}", Encoding.UTF8.GetString(payload));
        }

        static string GetSecurityToken(string role)
        {
            string roleClaimValue = abSwitch ? "A" : "B";
            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, Guid.NewGuid().ToString()),
                new Claim(roleClaimType, roleClaimValue)
            };

            JsonWebToken token = new JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, 20.0);
            return token.ToString();
        }
    }
}
