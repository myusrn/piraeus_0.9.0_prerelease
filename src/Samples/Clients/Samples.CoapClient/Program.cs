using Piraeus.Clients.Coap;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.CoapClient
{
    class Program
    {

        static string audience = "http://www.skunklab.io/";
        static string issuer = "http://www.skunklab.io/";
        static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static string nameClaimType = "http://www.skunklab.io/name";
        static string roleClaimType = "http://www.skunklab.io/role";

        static string endpoint = "ws://localhost:4163/api/connect";
        //static string endpoint = "ws://localhost:3111/api/connect";
        static IChannel channel;
        static CancellationTokenSource source;
        static bool abSwitch;
        static string resourceA = "http://www.skunklab.io/resourcea";
        static string resourceB = "http://www.skunklab.io/resourceb";

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();
            Console.WriteLine("CoAP Client --WooHoo!!!---");
            Console.ReadKey();
            Console.Write("Select a role (A/B) ? ");
            abSwitch = Console.ReadLine().ToUpperInvariant() == "A";

            string token = GetSecurityToken(abSwitch ? "A" : "B");
            
            channel = ChannelFactory.Create(new Uri(endpoint), token, "coapv1", new WebSocketConfig(), source.Token);
            channel.OnStateChange += Channel_OnStateChange1;
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError;
            

            CoapConfig config = new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe, false, 180.0,30.0,1.5,2,1,4.0,1.0,100.0);
            PiraeusCoapClient coapClient = new PiraeusCoapClient(config, channel);

            string subResource = abSwitch ? resourceB : resourceA;

            Task subTask = coapClient.ObserveAsync(subResource, Observe);
            Task.WhenAll(subTask);

            Console.WriteLine("Subscribed {0}", subResource);


            string pubResource = abSwitch ? resourceA : resourceB;

           

            Console.WriteLine("Press key to send message");
            Console.ReadKey();

            bool trysend = true;
            while (trysend)
            {
                //Console.Write("Unobserve (Y/N) ? ");
                //if(Console.ReadLine().ToLowerInvariant() == "y")
                //{
                //    Task unobserveTask = coapClient.UnobserveAsync(subResource);
                //    Task.WhenAll(unobserveTask);
                //}

                int index = 0;
                Console.Write("Send another (Y/N) ? ");
                trysend = Console.ReadLine().ToLowerInvariant() == "y";

                while(trysend)
                {
                    while (index < 10)
                    {
                        if (trysend)
                        {
                            string message = String.Format("Hi from {0} {1}", pubResource, index++);
                            //TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
                            Task pubTask = coapClient.PublishAsync(pubResource, "text/plain", Encoding.UTF8.GetBytes(message), true, Response);
                            Task.WhenAll(pubTask);
                            Console.WriteLine(index);
                            //Task.WaitAll(pubTask);
                            
                        }
                    }

                    index = 0;
                    Console.Write("Send another (Y/N) ? ");
                    trysend = Console.ReadLine().ToLowerInvariant() == "y";
                }
                

            }

            source.Cancel();
            Console.WriteLine("Finished.");
            Console.ReadKey();

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

        private static void Observe(CodeType code, string contentType, byte[] payload)
        {
            if (payload != null)
            {
                Console.WriteLine("Observe with message {0}", Encoding.UTF8.GetString(payload));
            }
            else
            {
                Console.WriteLine("Observer with Code {0}", code);
            }
        }

        private static void Response(CodeType code, string contentType, byte[] payload)
        {
            Console.WriteLine("Response with Code = {0}", code);
        }


        private static void Channel_OnStateChange1(object sender, ChannelStateEventArgs e)
        {
            Console.WriteLine("Channel State = {0}", e.State);
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
