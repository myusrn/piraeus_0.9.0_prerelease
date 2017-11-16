using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class NamespaceController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post(string ns)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();            
            manager.CreateNamespace(key, ns);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public string[] Get()
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetNamespaces(key);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string ns)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemoveNamespace(key, ns);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public bool HasNamespace(string ns)
        {
            StorageManager manager = new StorageManager();
            return manager.HasNamespace(ns);
        }
    }
}
