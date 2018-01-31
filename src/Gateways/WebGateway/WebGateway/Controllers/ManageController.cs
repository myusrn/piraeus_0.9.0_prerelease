using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class ManageController : ApiController
    {
        public ManageController()
        {
            bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            if (!dockerized)
            {
                string codesString = ConfigurationManager.AppSettings["managementCodes"];
                codes = codesString.Split(new char[] { ' ' });
            }
            else
            {
                string cstring = System.Environment.GetEnvironmentVariable("MGMT_API_SECURITY_CODE");
                codes = cstring.Split(new char[] { ' ' });
            }


            if (!Orleans.GrainClient.IsInitialized)
            {                
                if (!dockerized)
                {                    
                    OrleansClientConfig.TryStart("ManageController");                    
                }
                else
                {                    
                    OrleansClientConfig.TryStart("ManageController", System.Environment.GetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME"));                    
                }

                Task task = ServiceIdentityConfig.Configure();
                Task.WhenAll(task);
            }

            
        }

        private string[] codes;
        /// <summary>
        /// Return a security token for the Management API
        /// </summary>
        /// <param name="code">Management code</param>
        /// <returns>JWT security token</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string code)
        {
            //string codes = ConfigurationManager.AppSettings["managementCodes"];
            //string[] items = codes.Split(new char[] { ' ' });
            //foreach (string item in items)
            //{
            //    if (item.ToLowerInvariant() == code.ToLowerInvariant())
            //    {
            //        string token = await GetJwtTokenAsync();

            //        return Request.CreateResponse<string>(HttpStatusCode.OK, token);
            //    }
            //}

            try
            {
                foreach (string codeString in codes)
                {
                    if (codeString.ToLowerInvariant() == code.ToLowerInvariant())
                    {
                        string token = await GetJwtTokenAsync();
                        return Request.CreateResponse<string>(HttpStatusCode.OK, token);
                    }
                }

                return Request.CreateResponse<string>(HttpStatusCode.Unauthorized, "Invalid code.");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        /// <summary>
        /// Creates JWT token with "manage" claim to authz access to management API
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetJwtTokenAsync()
        {
            string audience = null;
            string issuer = null;
            string key = null;
            string nameClaimType = null;
            string roleClaimType = null;
            string roleClaimValue = null;

            bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            if(dockerized)
            {
                audience = System.Environment.GetEnvironmentVariable("MGMT_API_AUDIENCE");
                issuer = System.Environment.GetEnvironmentVariable("MGMT_API_ISSUER");
                key = System.Environment.GetEnvironmentVariable("MGMT_API_SYMMETRICKEY");
                nameClaimType = System.Environment.GetEnvironmentVariable("MGMT_API_NAME_CLAIM_TYPE");
                roleClaimType = System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_TYPE");
                roleClaimValue = System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_VALUE");

            }
            else
            {
                audience = ConfigurationManager.AppSettings["audience"];
                key = ConfigurationManager.AppSettings["symmetricKey"];
                issuer = ConfigurationManager.AppSettings["issuer"];
                nameClaimType = ConfigurationManager.AppSettings["nameClaimType"]; 
                roleClaimType = ConfigurationManager.AppSettings["roleClaimType"]; 
                roleClaimValue = ConfigurationManager.AppSettings["roleClaimValue"]; 
            }

            
            double lifetimeMinutes = 60.0;

            List<Claim> claimSet = new List<Claim>()
            {
                new Claim(nameClaimType, Guid.NewGuid().ToString()),
                new Claim(roleClaimType, roleClaimValue)
            };

            JsonWebToken jwt = new JsonWebToken(key, claimSet, lifetimeMinutes, issuer, audience);

            return await Task.FromResult<string>(jwt.ToString());
        }
    }
}
