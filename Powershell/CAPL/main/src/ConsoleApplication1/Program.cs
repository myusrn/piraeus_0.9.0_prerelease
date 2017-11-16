using Newtonsoft.Json;
using Piraeus.Services.Common;
using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            

            string Key = "9ebc74d6bf8f41e099e5479fc27a4c98";
            string ServiceUrl = "http://localhost:32993";
            string Description = "My Test App";
            string AppId = "myappid";

            string token = UserToken.Get(Key,ServiceUrl);
            string url = String.Format("{0}/api/app?appId={1}", ServiceUrl, AppId);

            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();

            Console.ReadKey();
        }
    }
}
