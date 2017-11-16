using Piraeus.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace AuthzPolicyServer.Controllers
{
    public class TokenRefreshController : ApiController
    {
        [HttpGet]
        public string Get()
        {
            StorageManager manager = new StorageManager();

            ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;
            ClaimsIdentity ident = prin.Identity as ClaimsIdentity;

            Claim refreshClaim = ident.Claims.First((c) =>
            {
                return c.Type == AccessControlConfiguration.TokenRefreshClaimType;
            });

            if(refreshClaim != null &&
                Convert.ToBoolean(ident.FindFirst(AccessControlConfiguration.TokenRefreshClaimType).Value))
            {                
                string id = ident.FindFirst(AccessControlConfiguration.NameClaimType).Value;

                if(string.IsNullOrEmpty(id))
                {
                    throw new SecurityException("Token has no expected identifier.");
                }


                string key = manager.GetSecurityKeyForRefresh(id);
                if(!string.IsNullOrEmpty(key))
                {
                    return manager.GetSecurityToken(key);
                }
                else
                {
                    throw new SecurityException("Security key not found.");
                }
            }
            else
            {
                throw new SecurityException("Cannot refresh token.");
            }
        }
    }
}
