using Capl.Authorization;
using Piraeus.Services.Common;
using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class PolicyController : ApiController
    {
        [HttpGet]  //this needs to using content-type application/xml
        public AuthorizationPolicy Get(string policyId)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetPolicy(key, policyId);
        }

        [HttpPost]
        public HttpResponseMessage Post(Policy policy)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.CreatePolicy(key, policy.GetPolicy(), policy.Description);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut]
        public HttpResponseMessage Put(Policy policy)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.UpdatePolicy(key, policy.GetPolicy(), policy.Description);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string policyId)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemovePolicy(key, policyId);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
