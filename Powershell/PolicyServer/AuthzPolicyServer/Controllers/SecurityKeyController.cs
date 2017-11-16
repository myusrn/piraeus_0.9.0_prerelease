using SkunkLab.Common;
using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class SecurityKeyController : ApiController
    {
        [HttpPost]
        public string Post(SecurityKey securityKey)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.CreateSecurityKey(key, securityKey.IsAdmin, securityKey.Description, securityKey.Claims);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string id)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemoveSecurityKey(key, id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut]
        public HttpResponseMessage Put(SecurityKey securityKey)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            
        }



    }
}
