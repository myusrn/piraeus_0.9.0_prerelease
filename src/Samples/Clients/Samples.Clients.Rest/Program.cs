using Piraeus.Clients.Rest;
using SkunkLab.Channels.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Clients.Rest
{
    class Program
    {
        



        static int index;
        static string endpoint;

        static CancellationTokenSource source;

        static string publishResource;
        static string observeResource;
        static string role;
        static string clientName;
        static RestClient client;


        static void Main(string[] args)
        {
            WriteHeader();
            
            SelectClientRole();
            string securityToken = GetSecurityToken();  //get the security token with a unique name
            SetResources(); //setup the resources for pub and observe based on role.
            SelectEndpoint();

            source = new CancellationTokenSource();

            //create HTTP Observer
            HttpObserver observer = new HttpObserver(new Uri(observeResource));
            observer.OnNotify += Observer_OnNotify;

            //create the REST client
            client = new RestClient(endpoint, securityToken, new HttpObserver[] { observer }, source.Token);

            Task stubTask = Task.Delay(1).ContinueWith(SendMessages);
            Task.WaitAll(stubTask);

            source.Cancel();
            Console.WriteLine("press any key to terminate");
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
                    Task pubTask = client.SendAsync(publishResource, "text/plain", payload);
                    Task.WhenAll(pubTask);

                    if (delay > 0)
                    {
                        Task.Delay(delay).Wait();
                    }
                }

                SendMessages(task);
            }
        }

        private static void Observer_OnNotify(object sender, SkunkLab.Channels.ObserverEventArgs args)
        {
            if(args.Message != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Encoding.UTF8.GetString(args.Message));
                Console.ResetColor();
            }
        }


        

        #region Security Token

        
        private static List<Claim> GetClaims()
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ConfigurationManager.AppSettings["nameClaimType"], Guid.NewGuid().ToString()));
            claims.Add(new Claim(ConfigurationManager.AppSettings["roleClaimType"], role));            

            return claims;
        }

        static void SetResources()
        {
            string resource1 = ConfigurationManager.AppSettings["resource1"];
            string resource2 = ConfigurationManager.AppSettings["resource2"];
            publishResource = role == "A" ? resource1 : resource2;
            observeResource = role == "A" ? resource2 : resource1;
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

        public static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            return jwt.ToString();
        }
      

        #endregion

        #region UX Inputs
       

       
      


        static void SelectClientRole()
        {
            Console.WriteLine();
            Console.Write("Enter Role for this client (A/B) ? ");
            role = Console.ReadLine().ToUpperInvariant();
            if (role != "A" && role != "B")
                SelectClientRole();
        }

        static void SelectEndpoint()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Enter hostname, IP, or Enter for localhost ? ");
            string hostnameOrIP = Console.ReadLine();

            IPAddress address = null;
            bool isIP = IPAddress.TryParse(hostnameOrIP, out address);
            string authority = isIP ? address.ToString() : String.IsNullOrEmpty(hostnameOrIP) ? "localhost" : hostnameOrIP;

            int port = Convert.ToInt32(ConfigurationManager.AppSettings["localhostPort"]);
            endpoint = authority.Contains("localhost") ?
                                String.Format("http://{0}:{1}/api/connect", authority, port) :
                                String.Format("http://{0}/api/connect", authority);
            
        }

        static void SelectClientName()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter unique client name ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            clientName = Console.ReadLine();
            Console.ResetColor();
        }


        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- REST Client ---");
            Console.WriteLine("press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        #endregion
    }
}
