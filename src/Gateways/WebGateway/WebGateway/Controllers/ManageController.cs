using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebGateway.Controllers
{
    public class ManageController : ApiController
    {
        /// <summary>
        /// Return a security token for the Management API
        /// </summary>
        /// <param name="code">Management code</param>
        /// <returns>JWT security token</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string code)
        {
            string codes = ConfigurationManager.AppSettings["managementCodes"];
            string[] items = codes.Split(new char[] { ' ' });
            foreach (string item in items)
            {
                if (item.ToLowerInvariant() == code.ToLowerInvariant())
                {
                    string token = await GetJwtTokenAsync();

                    return Request.CreateResponse<string>(HttpStatusCode.OK, token);
                }
            }

            return Request.CreateResponse<string>(HttpStatusCode.Unauthorized, "Invalid code.");
        }


        /// <summary>
        /// Creates JWT token with "manage" claim to authz access to management API
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetJwtTokenAsync()
        {
            Uri address = new Uri(ConfigurationManager.AppSettings["audience"]);
            string key = ConfigurationManager.AppSettings["symmetricKey"];
            string issuer = ConfigurationManager.AppSettings["issuer"];
            double lifetimeMinutes = 60.0;

            List<Claim> claimSet = new List<Claim>()
            {
                new Claim("http://www.skunklab.io/piraeus/name", Guid.NewGuid().ToString()),
                new Claim(ConfigurationManager.AppSettings["matchClaimType"], ConfigurationManager.AppSettings["matchClaimValue"])
            };

            JsonWebToken jwt = new JsonWebToken(address, key, issuer, claimSet, lifetimeMinutes);

            return await Task.FromResult<string>(jwt.ToString());
        }
    }
}
