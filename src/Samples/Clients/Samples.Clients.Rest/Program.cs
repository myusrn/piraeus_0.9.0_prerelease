using Piraeus.Clients.Rest;
using SkunkLab.Channels.Http;
using SkunkLab.Security.Tokens;
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
        private static string audience = "http://www.skunklab.io/";
        private static string issuer = "http://www.skunklab.io/";
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static string nameClaimType = "http://www.skunklab.io/name";
        private static string roleClaimType = "http://www.skunklab.io/role";

        static string CT_TEXT = "text/plain";
        static string CT_JSON = "application/json";
        static string CT_XML = "application/xml";
        static string CT_BYTES = "application/octet-stream";


        static int index;
        static string endpoint;

        static string resourceA;
        static string resourceB;
        static CancellationTokenSource source;
        static string contentType;

    
        static string indexClaimType;
        static string indexClaimValue;


        static string publishResource;
        static string observeResource;
        static string role;
        static string clientName;


        static void Main(string[] args)
        {
            WriteHeader();
            
            SelectClientRole();
            SelectClientName();
            SelectEndpoint();
            SetResources();

            string token = GetSecurityToken();
            source = new CancellationTokenSource();

            //create HTTP Observer
            HttpObserver observer = new HttpObserver(new Uri(observeResource));
            observer.OnNotify += Observer_OnNotify;

            //create the REST client
            RestClient client = new RestClient(endpoint, token, new HttpObserver[] { observer }, source.Token);
                        
            Console.WriteLine("Press any key to send a message !");
            Console.ReadKey();

            bool sending = true;
            while (sending)
            {
                //List<KeyValuePair<string, string>> indexes = null;
                index++;
                byte[] message = Encoding.UTF8.GetBytes(String.Format("{0} sent message {1}", clientName, index));                
                Task sendTask = client.SendAsync(publishResource, contentType, message);
                Task.WhenAll(sendTask);
                Console.Write("Do you want to send another message (Y/N) ? ");               
                sending = Console.ReadLine().ToLowerInvariant() == "y";
            }

            source.Cancel();
            Console.WriteLine("press any key to terminate");
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
            claims.Add(new Claim(nameClaimType, Guid.NewGuid().ToString()));
            claims.Add(new Claim(roleClaimType, role));

            if(indexClaimType != null)
            {
                claims.Add(new Claim(indexClaimType, indexClaimValue));
            }

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
        static void SelectContentType()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select content type for messages transmitted...");
            Console.WriteLine("(1) {0}", CT_TEXT);
            Console.WriteLine("(2) {0}", CT_JSON);
            Console.WriteLine("(3) {0}", CT_XML);
            Console.WriteLine("(4) {0}", CT_BYTES);
            Console.Write("Enter content-type code ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            string codeString = Console.ReadLine();
            Console.ResetColor();

            int val = 0;
            if(Int32.TryParse(codeString, out val))
            {
                if(val > 0 && val < 5)
                {
                    if(val == 1)
                    {
                        contentType = CT_TEXT;
                    }
                    else if(val == 2)
                    {
                        contentType = CT_JSON;
                    }
                    else if(val == 3)
                    {
                        contentType = CT_XML;
                    }
                    else
                    {
                        contentType = CT_BYTES;
                    }
                }
                else
                {
                    SelectContentType();
                }
            }
            else
            {
                SelectContentType();
            }
        }

        static void AddIndexClaim()
        {
            Console.Write("Do you want to add a claim for indexing (Y/N) ? ");
            if(Console.ReadLine().ToLowerInvariant() == "y")
            {
                Console.Write("Enter claim type to index  ? ");
                indexClaimType = Console.ReadLine();
                Console.Write("Enter claim value to index ? ");
                indexClaimValue = Console.ReadLine();                
            }
        }
        static void SelectObserveResource()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter Resource URI to observe messages ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            resourceB = Console.ReadLine();
            Console.ResetColor();
        }
                

        static void SelectClientRole()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter a role for this client (A/B) ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            role = Console.ReadLine().ToUpper();
            Console.ResetColor();

            if (!(role == "A" || role == "B"))
            {
                SelectClientRole();
            }
            else if(role == "A")
            {
                resourceA = "http://www.skunklab.io/resource-a";
                resourceB = "http://www.skunklab.io/resource-b";
               
            }
            else
            {
                resourceB = "http://www.skunklab.io/resource-a";
                resourceA = "http://www.skunklab.io/resource-b";
            }
           
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
