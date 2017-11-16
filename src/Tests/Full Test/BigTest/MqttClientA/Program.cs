using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Piraeus.Clients.Mqtt;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Mqtt;

namespace MqttClientA
{
    class Program
    {
        static string endpointUriString = "ws://localhost:3111/api/connect";
        static string resourceUriString = "http://www.skunklab.io/resource1";
        static IChannel channel;
        static CancellationTokenSource source;
        static void Main(string[] args)
        {
            Console.WriteLine("MQTT Client A");
            Console.ReadKey();

            OpenChannel();
            MqttConfig config = new MqttConfig(null);

            GenericMqttDispatcher dispatcher = new GenericMqttDispatcher();
            dispatcher.Register(resourceUriString, new Action<string, string, byte[]>(ReceivedMessage));
            PiraeusMqttClient client = new PiraeusMqttClient(config, channel, dispatcher);

            Task<ConnectAckCode> task = ConnectAsync(client);
            Task.WhenAll<ConnectAckCode>(task);

            bool sending = true;

            Console.WriteLine("Send a message ? ");
            Console.ReadKey();

            int index = 0;
            while (sending)
            {
                Task pubTask = client.Publish(QualityOfServiceLevelType.AtLeastOnce, resourceUriString, "text/plain", Encoding.UTF8.GetBytes(String.Format("hello from mqtt {0}", index++)));
                Task.WhenAll(pubTask);

                Console.WriteLine("Send another message (Y/N) ? ");
                if (Console.ReadLine().ToLowerInvariant() != "y")
                    sending = false;
            }

            source.Cancel();
            Thread.Sleep(3000);            
        }


        static async Task<ConnectAckCode> ConnectAsync(PiraeusMqttClient client)
        {
            ConnectAckCode code = await client.ConnectAsync("myclientIdA", "JWT", GetSecurityToken(), 180);
            Console.WriteLine("Connection is --- {0} ---", code);

            await client.SubscribeAsync("http://www.skunklab.io/resource2", QualityOfServiceLevelType.AtLeastOnce, new Action<string, string, byte[]>(ReceivedMessage));

            return code;


        }

        static void ReceivedMessage(string resourceUriString, string contentType, byte[] payload)
        {
            Console.WriteLine("Message {0}", Encoding.UTF8.GetString(payload));
        }


        static void OpenChannel()
        {
            string securityToken = GetSecurityToken();
            WebSocketConfig config = new WebSocketConfig();
            source = new CancellationTokenSource();
            channel = ChannelFactory.Create(new Uri(endpointUriString), securityToken, "mqtt", config, source.Token);

            Task openTask = channel.OpenAsync();
            Task.WaitAll(openTask);

            if(!channel.IsConnected)
            {
                Thread.Sleep(100);
            }

            Task receiveTask = channel.ReceiveAsync();
            Task.WhenAll(receiveTask);


        }

        static string GetSecurityToken()
        {
            PiraeusConfig config = Piraeus.Configuration.PiraeusConfigManager.Settings;

            List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>() { new System.Security.Claims.Claim("http://www.skunklab.io/name", "testuser2"), new System.Security.Claims.Claim("http://www.skunklab.io/role", "pub") };
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(config.Security.Client.Audience), config.Security.Client.SymmetricKey, config.Security.Client.Issuer, claims);

            return jwt.ToString();
        }

        
    }
}
