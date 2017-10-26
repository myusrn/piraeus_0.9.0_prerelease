using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Security.Tokens;

namespace RestClientTest
{
    class Program
    {
        private static string secret = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static void Main(string[] args)
        {
            Console.WriteLine("REST MESSAGE...");
            Console.ReadKey();
            string token = GetSecurityToken();
            Piraeus.Clients.Rest.RestClient restClient = new Piraeus.Clients.Rest.RestClient(new Uri("http://localhost:21949/api/connect"), "http://www.example.org/resource1", "text/plain", token);
            Task task = restClient.SendAsync(Encoding.UTF8.GetBytes("hello"));
            Task.WhenAll(task);
            Console.WriteLine("Sent");
            Console.ReadKey();

        }

        static string GetSecurityToken()
        {
            string audience = "http://www.example.org/";
            string issuer = "http://www.example.org/";
            
            List<Claim> list = new List<Claim>();
            list.Add(new Claim("http://www.example.org/name", "testuser"));
            list.Add(new Claim("http://www.example.org/claim/index1", "value1"));

            JsonWebToken jwt = new JsonWebToken(new Uri(audience), secret, issuer, list, 30.0);
            return jwt.ToString();
        }
    }
}
