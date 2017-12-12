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

            Console.WriteLine("MQTT Client press any key to continue.");
            Console.ReadKey();

            Console.Write("Select a role (A/B) ? ");
            abSwitch = Console.ReadLine().ToUpperInvariant() == "A";

            string token = GetSecurityToken(abSwitch ? "A" : "B");

            channel = ChannelFactory.Create(new Uri(endpoint), token, "mqtt", new WebSocketConfig(), source.Token);
            channel.OnStateChange += Channel_OnStateChange1;
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError;
            //Task task = channel.OpenAsync();
            //Task.WaitAll(task);

            //Task t = channel.ReceiveAsync();
            //Task.WhenAll(t);

            string pubResource = abSwitch ? resourceA : resourceB;
            string subResource = abSwitch ? resourceB : resourceA;

            MqttConfig config = new MqttConfig(null);
            PiraeusMqttClient client = new PiraeusMqttClient(config, channel);
            client.RegisterTopic(subResource, Subscription);
            ConnectAckCode code = client.ConnectAsync("sessionId", "JWT", token, 90).Result;
            Console.WriteLine("Connected with Ack Code {0}", code);

            Task subTask = client.SubscribeAsync(subResource, QualityOfServiceLevelType.AtLeastOnce, Subscription);
            Task.WhenAll(subTask);

            Task pubTask = client.Publish(QualityOfServiceLevelType.AtLeastOnce, pubResource, "text/plain", Encoding.UTF8.GetBytes("Hi!"));
            Task.WhenAll(pubTask);

            Console.WriteLine("Waiting for messages...because I was too lazy to write a sender :-)");
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

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error {0}", e.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel closed");
            Console.ResetColor();
        }

        private static void Channel_OnStateChange1(object sender, ChannelStateEventArgs e)
        {
            Console.WriteLine("Channel State = {0}", e.State);
        }
    }
}
