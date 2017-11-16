using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Piraeus.Clients.Coap;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using System.Security.Claims;
using Client.Common;

namespace ClientA
{
    class Program
    {
        private static string audience = "http://www.skunklab.io/";
        private static string issuer = "http://www.skunklab.io/";       
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static string nameClaimType = "http://www.skunklab.io/name";
        private static string roleClaimType = "http://www.skunklab.io/role";
        private static string nameClaimValue = Guid.NewGuid().ToString();
        private static string roleClaimVlaue = "pub";
        private static string coapHostname = "www.skunklab.io";
        private static string resourceUriString1 = "http://www.skunklab.io/resource1";
        private static string resourceUriString2 = "http://www.skunklab.io/resource2";
        private static string webSocketEndpoint = "ws://localhost:3111/api/connect";
        private static string httpEndpoint = "http://localhost:3111/api/connect";
        static IChannel channel;
        private static CancellationTokenSource source;

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start client A");
            Console.ReadKey();
            //source to cancel channel
            source = new CancellationTokenSource();

            //claims for security token
            List<Claim> claims = GetClaims();
            //security token to open Web socket
            string securityToken = SymmetricKeySecurityToken.CreateJwt(symmetricKey, issuer, audience, 20.0, claims);

            //configuration for Web socket
            WebSocketConfig config = Configuration.GetWebSocketConfig();

            //get Web socket opened and connected using CoAP subprotocol
            channel = Channels.GetChannel(webSocketEndpoint, securityToken, "coapv1", config, source.Token);

            if(!channel.IsConnected)
            {
                Console.WriteLine("Channel connected is {0}", "FALSE");
            }


            //create the CoAP client for Piraeus
            PiraeusCoapClient coapClient = new PiraeusCoapClient(Configuration.GetCoapConfig(coapHostname), channel, null);
           
            //setup an observer for a resource using CoAP observe option
            Task task = coapClient.ObserveAsync(resourceUriString2, new Action<CodeType, string, byte[]>(ObserveAction));
            Task.WhenAll(task);

            Console.WriteLine("press any key to send a message");
            Console.ReadKey();

            bool sending = true;
            int index = 0;
            while(sending)
            {
                Task sendTask = coapClient.PublishAsync(resourceUriString1,
                        "text/plain",
                        Encoding.UTF8.GetBytes(String.Format("hello from Client A - CoAP {0}", index++)),
                        false,
                        new Action<CodeType, string, byte[]>(ResponseAction));

                Task.WhenAll(sendTask);

                Console.WriteLine("Send another ? ");
                sending = Console.ReadLine().ToLowerInvariant() == "y";

            }
            

            Console.WriteLine("Press any key to terminate");                
            Console.ReadKey();
            source.Cancel();
            Thread.Sleep(1000);
        }
        

        private static void ObserveAction(CodeType code, string contentType, byte[] message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (message != null)
            {
                Console.WriteLine("Received : Code '{0}'  message '{1}'", code, Encoding.UTF8.GetString(message));
            }
            else
            {
                Console.WriteLine("Received : Code '{0}'", code);
            }
            Console.ResetColor();
        }

        private static void ResponseAction(CodeType code, string contentType, byte[] message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Response Code {0}", code);
            Console.ResetColor();
        }   

        static List<Claim> GetClaims()
        {
            return new List<Claim>()
            {
                new Claim(nameClaimType, nameClaimValue),
                new Claim(roleClaimType, roleClaimVlaue)
            };
        }


    }
}
