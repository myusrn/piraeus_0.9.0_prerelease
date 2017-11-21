using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestClientA
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpointUriString = "";
            string resourceUriString = "";
            string contentType = "text/plain";
            string securityToken = "";
            Piraeus.Clients.Rest.RestClient client = new Piraeus.Clients.Rest.RestClient(new Uri(""), "resource", "contentType", "token", null, CancellationToken.None);
            Task task = client.SendAsync(Encoding.UTF8.GetBytes("hello from Rest Client"));
            Task.WhenAll(task);
        }
    }
}
