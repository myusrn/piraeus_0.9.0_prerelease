using Piraeus.Clients.Rest;
using SkunkLab.Channels.Http;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
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
        static string endpoint = "http://localhost:1733/api/connect";

        static string resourceA;
        static string resourceB;
        static CancellationTokenSource source;
        static string contentType;

        static string clientName;
        static string role;
        static string indexClaimType;
        static string indexClaimValue;


        static void Main(string[] args)
        {
            WriteHeader();
            SelectEndpoint();
            SelectClientName();
            SelectClientRole();
            //AddIndexClaim();
            //SelectContentType();
            string token = GetSecurityToken(symmetricKey, issuer, audience, 60.0, GetClaims());
            source = new CancellationTokenSource();

            //create HTTP Observer
            HttpObserver observer = new HttpObserver(new Uri(resourceB));
            observer.OnNotify += Observer_OnNotify;

            //create the REST client
            RestClient client = new RestClient(endpoint, token, new HttpObserver[] { observer }, source.Token);
            

            
            
            Console.WriteLine("Press any key to send a message !");
            Console.ReadKey();

            bool sending = true;
            while (sending)
            {
                List<KeyValuePair<string, string>> indexes = null;
                index++;
                byte[] message = Encoding.UTF8.GetBytes(String.Format("{0} sent message {1}", clientName, index));
                //Console.Write("Do you want to send with an index (Y/N) ? ");
                //if(Console.ReadLine().ToLowerInvariant() == "y")
                //{
                //    Console.Write("Enter index name ? ");
                //    string indexName = Console.ReadLine().ToLowerInvariant();
                //    Console.Write("Enter index value ? ");
                //    string indexValue = Console.ReadLine().ToLowerInvariant();
                //    indexes = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(indexName, indexValue) });
                //}

                Task sendTask = client.SendAsync(resourceA, contentType, message);
                Task.WhenAll(sendTask);
                Console.Write("Do you want to send another message (Y/N) ? ");               
                sending = Console.ReadLine().ToLowerInvariant() == "y";
            }

            source.Cancel();
            Console.WriteLine("press any key to terminate");
        }

        private static void Observer_OnNotify(object sender, SkunkLab.Channels.ObserverEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- OBSERVED MESSAGE ---");
            Console.WriteLine("{0} message = {1} : content-type {2}", args.ResourceUri, Encoding.UTF8.GetString(args.Message), args.ContentType);
            Console.ResetColor();
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

        private static string GetSecurityToken(string key, string issuer, string audience, double lifetimeMinutes, List<Claim> claims)
        {
            JsonWebToken token = new JsonWebToken(new Uri(audience), key, issuer, claims, lifetimeMinutes);
            return token.ToString();
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
                resourceA = "http://www.skunklab.io/resourcea";
                resourceB = "http://www.skunklab.io/resourceb";
               
            }
            else
            {
                resourceB = "http://www.skunklab.io/resourcea";
                resourceA = "http://www.skunklab.io/resourceb";
            }
           
        }

        static void SelectEndpoint()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter REST endpoint or Enter for default ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            string url = Console.ReadLine();
            endpoint = String.IsNullOrEmpty(url) ? endpoint : url;
            Console.ResetColor();
        }

        static void SelectClientName()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter unique name for this client ? ");
            Console.ForegroundColor = ConsoleColor.Green;
            clientName = Console.ReadLine();
            Console.ResetColor();
        }


        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--- REST Client Sample ---");
            Console.WriteLine("press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        #endregion
    }
}
