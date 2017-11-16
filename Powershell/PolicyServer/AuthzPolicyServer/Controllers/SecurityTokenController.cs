using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class SecurityTokenController : ApiController
    {
        [HttpGet]
        public string Get(string key)
        {
            StorageManager manager = new StorageManager();
            return manager.GetSecurityToken(key);
        }

    }
}
