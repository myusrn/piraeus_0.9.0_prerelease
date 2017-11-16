using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestServices
{
    public class UserToken
    {
        public static string Get(string key, string serviceUrl)
        {
            string url = String.Format("{0}/api/securitytoken", serviceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, key);
            RestRequest request = new RestRequest(builder);
            return request.Get<string>();
        }
    }
}
