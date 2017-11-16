using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SkunkLab.Common;
using Piraeus.Services.Storage;

namespace AuthzPolicyServer.Controllers
{
    public class AppIdentityController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post (AppIdentity identity)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.CreateApp(identity.Id, key, identity.Description, identity.Claims);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string id)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemoveApp(id, key);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        [HttpPut]
        public HttpResponseMessage Put(AppIdentity identity)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.UpdateApp(identity, key);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public AppIdentity Get(string id)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetApp(id, key);
        }
    }
}
