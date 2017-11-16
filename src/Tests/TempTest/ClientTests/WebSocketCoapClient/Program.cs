using Client.Common;
using Piraeus.Clients.Coap;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketCoapClient
{
    class Program
    {
        private static string clientId;
        private static string role;
        private static string address;

        private static string audience = "http://www.skunklab.io/";
        private static string issuer = "http://www.skunklab.io/";
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static string nameClaimType = "http://www.skunklab.io/name";
        private static string roleClaimType = "http://www.skunklab.io/role";

        private static string nameClaimValue;
        private static string roleClaimValue;
        private static string coapHostname = "www.skunklab.io";

        private static string resource1 = "http://www.skunklab.io/resource1";
        private static string resource2 = "http://www.skunklab.io/resource2";

        private static IChannel channel;
        private static CancellationTokenSource source;

        private static int index;

        static void Main(string[] args)
        {
            Console.Write("Enter client identifier ? ");
            clientId = Console.ReadLine();
            nameClaimValue = clientId;
            Console.WriteLine();
            Console.Write("Select role (A/B) ? ");
            
            role = Console.ReadLine().ToLowerInvariant();
            roleClaimValue = role == "a" ? "pub" : "sub";

            Console.WriteLine();
            Console.Write("Enter gateway address for Web socket ? ");
            address = Console.ReadLine();

            source = new CancellationTokenSource();

            //claims for security token
            List<Claim> claims = GetClaims();
            //security token to open Web socket
            string securityToken = SymmetricKeySecurityToken.CreateJwt(symmetricKey, issuer, audience, 20.0, claims);

            //configuration for Web socket
            WebSocketConfig config = Configuration.GetWebSocketConfig();

            //get Web socket opened and connected using CoAP subprotocol
            channel = Channels.GetChannel(address, securityToken, "coapv1", config, source.Token);

            //create the CoAP client for Piraeus
            PiraeusCoapClient coapClient = new PiraeusCoapClient(Configuration.GetCoapConfig(coapHostname), channel, null);

            //setup an observer for a resource using CoAP observe option
            //string observableResource = role == "b" ? resource1 : resource2;
            //string publishResource = role == "b" ? resource2 : resource1;
            //Task observeTask = coapClient.ObserveAsync(observableResource, new Action<CodeType, string, byte[]>(ObserveAction));
            //Task.WhenAll(observeTask);

            Task obTask = ObserveAsync(coapClient);
            Task.WhenAll(obTask);

            Console.WriteLine("-------------------------------------");
            Console.Write("Press Y to send a message ? ");
            bool sending = Console.ReadLine().ToLowerInvariant() == "y";
            while (sending)
            {
                //Task pubTask = coapClient.PublishAsync(publishResource, "text/plain", Encoding.UTF8.GetBytes(String.Format("Hello from {0} - {1}", clientId, index++)), false, new Action<CodeType, string, byte[]>(ResponseAction));
                //Task.WhenAll(pubTask);
                Task pubTask = PublishAsync(coapClient);
                Task.WhenAll(pubTask);

                Console.WriteLine("-------------------------------------");
                Console.Write("Press Y to send another message ? ");
                sending = Console.ReadLine().ToLowerInvariant() == "y";
            }
        }

        private static async Task ObserveAsync(PiraeusCoapClient client)
        {
            string observableResource = role == "b" ? resource1 : resource2;
            await client.ObserveAsync(observableResource, new Action<CodeType, string, byte[]>(ObserveAction));
        }

        private static async Task PublishAsync(PiraeusCoapClient client)
        {
            string publishResource = role == "b" ? resource2 : resource1;
            await client.PublishAsync(publishResource, "text/plain", Encoding.UTF8.GetBytes(String.Format("Hello from {0} - {1}", clientId, index++)), false, new Action<CodeType, string, byte[]>(ResponseAction));
        }

        private static void ObserveAction(CodeType code, string contentType, byte[] message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------------------------------");
            if (message != null)
            {
                Console.WriteLine("Received : Code '{0}'  message '{1}'", code, Encoding.UTF8.GetString(message));
            }
            else
            {
                Console.WriteLine("Received : Code '{0}'", code);
            }
            Console.WriteLine("-------------------------------------");
            Console.ResetColor();
        }

        private static void ResponseAction(CodeType code, string contentType, byte[] message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Response Code {0}", code);
            Console.ResetColor();
        }



        private static List<Claim> GetClaims()
        {
            return new List<Claim>()
            {
                new Claim(nameClaimType, nameClaimValue),
                new Claim(roleClaimType, roleClaimValue)
            };
        }
    }
}
