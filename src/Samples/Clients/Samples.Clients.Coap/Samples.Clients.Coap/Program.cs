using Piraeus.Clients.Coap;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Clients.Coap
{
    class Program
    {
        /****************************************************************************
         * This CoAP client will communicate over Web socket, TCP, or UDP channels 
         * based on configured resources, authorization policies, and demo TCP/UDP server 
         * on your local machine.  The sample does not use encrypted channels, e.g. TLSv1.2.
         * The sample is dependent on security token, which Piraeus uses to authenticate the client and
         * make deterministic the resources the client send and observe.
         * 
         * For this sample the following applies with respect to the channels and security token
         *    Web Sockets - The client places a JWT security token in the HTTP Authorize header
         *    TCP - The client encodes the security token in the first CoAP call in the CoAP URI string
         *    UDP - Same as TCP
         *
         * IMPORTANT NOTE:  You should run the LocalDemoSetup script in Powershell prior to running this 
         * sample to properly configure Piraeus to allow the client to send and receive from resources.
         * 
         * When choosing role "A" in this sample, it will the client to send to the Piraeus resource
         * http://www.skunklab.io/resourcea and receive/observe the resource http://www.skunklab.io/resourceb
         * When choosing role "B" in this sample, the send/recieve resources are reversed.
         * 
         ****************************************************************************************
         */


        //security configurations       
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

            //configure CoAP for the client
            CoapConfig config = new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe, false, 180.0, 30.0, 1.5, 2, 1, 4.0, 1.0, 100.0);

            //create the CoAP client
            PiraeusCoapClient client = null;

            if (channelNo == 1) //security token is in Authorize http header for Web socket
            {
                client = new PiraeusCoapClient(config, channel);
            }
            else 
            {
                //TCP and UDP channel; security token is passed in the CoAP URI on the first call
                client = new PiraeusCoapClient(config, channel, SecurityTokenType.JWT, securityToken);
            }

            //observe a resource
            try
            {
                Task task = client.ObserveAsync(observeResource, ObserveResource);
                Task.WaitAll(task);
                Console.WriteLine("Observing {0}", observeResource);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Observe failed");
                Console.WriteLine(ex.InnerException.Message);
                goto endsample;
            }

            

            SendMessages(client);

            source.Cancel();

            endsample:
            Console.WriteLine("client is closed...");
            Console.ReadKey();
        }

       

        private static IChannel GetChannel(int num, string securityToken)
        {
            if(num == 1)
            {
                Console.Write("Enter Web Socket URL or Enter for default ? ");
                string url = Console.ReadLine();

                if (String.IsNullOrEmpty(url))
                {
                    return ChannelFactory.Create(new Uri(endpoint), securityToken, "coapv1", new WebSocketConfig(), source.Token);
                }
                else
                {
                    return ChannelFactory.Create(new Uri(url), securityToken, "coapv1", new WebSocketConfig(), source.Token);
                }
            }
            else if (num == 2)
            {
                Console.Write("Enter TCP remote hostname or Enter for default ? ");
                string hostname = Console.ReadLine();
                hostname = String.IsNullOrEmpty(hostname) ? "localhost" : hostname;
                return ChannelFactory.Create(true, hostname, 5684, 1024, 2048, source.Token);
            }
            else if(num == 3)
            {
                Console.Write("Enter UDP remote hostname or Enter for default ? ");
                string hostname = Console.ReadLine();
                hostname = String.IsNullOrEmpty(hostname) ? "localhost" : hostname;
                Console.Write("Enter UDP port for this client to use ? ");
                int port = Int32.Parse(Console.ReadLine());
                return ChannelFactory.Create(port, hostname, 5683, source.Token);
            }

            return null;
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel closed");
            Console.ResetColor();
        }

        static void PublishResponse(CodeType code, string contentType, byte[] payload)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Publish response code {0}", code);
            Console.ResetColor();
        }
        static void ObserveResource(CodeType code, string contentType, byte[] payload)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (payload != null)
            {
                Console.WriteLine(Encoding.UTF8.GetString(payload));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(code);
                Console.ResetColor();
            }
            Console.ResetColor();
        }

        static void SendMessages(PiraeusCoapClient client)
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
                    Task pubTask = client.PublishAsync(publishResource, contentType, payload, false, PublishResponse);
                    Task.WhenAll(pubTask);

                    if (delay > 0)
                    {
                        Task.Delay(delay).Wait();
                    }
                }

                SendMessages(client);
            }
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
        

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Channel State {0}",e.State);
            Console.ResetColor();
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

        public static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            return jwt.ToString();
        }
        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--- CoAP Client ---");
            Console.WriteLine("press any key to continue...");
            Console.WriteLine();
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}
