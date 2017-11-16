using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class AcctController : ApiController
    {

        [HttpPost]
        public string Post(string ns)
        {
            StorageManager manager = new StorageManager();
            return manager.CreateAccount(ns);
        }

        [HttpGet]
        public string Get()
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetAccount(key);
        }

        [HttpDelete]
        public HttpResponseMessage Delete()
        {            
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemoveAccount(key);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
