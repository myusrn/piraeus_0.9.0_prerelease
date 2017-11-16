using Piraeus.Services.Common;
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
    public class AppController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post(App app)
        {            
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();

            manager.CreateApp(app.Id, key, app.Description, app.Claims);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public string[] Get()
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetApps(key);
        }

        [HttpGet]
        public App Get(string appId)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            return manager.GetApp(appId, key);
        }

        [HttpPut]
        public HttpResponseMessage Put(App app)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.UpdateApp(app, key);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string appId)
        {
            StorageManager manager = new StorageManager();
            string key = manager.GetSecurityKey();
            manager.RemoveApp(appId, key);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
