using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class ServiceController : ApiController
    {
        public ServiceController()
        {
            if (!Orleans.GrainClient.IsInitialized)
            {
                bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                if (!dockerized)
                {
                    OrleansClientConfig.TryStart("ServiceController");
                }
                else
                {
                    string hostname = ConfigurationManager.AppSettings["dnsHostEntry"];
                    OrleansClientConfig.TryStart("ServiceController", hostname);
                }
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> ConfigureIdentity(IdentityConfig config)
        {
            try
            {
                X509Certificate2 certificate = null;

                if (config.Certificate != null)
                {
                    certificate = new X509Certificate2(config.Certificate);                
                }

                await GraphManager.SetServiceIdentityAsync(config.Claims, certificate);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ConfigureAudit(string tableName, string connectionString)
        {
            try
            {
                await GraphManager.SetAuditConfigAsync(connectionString, tableName);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
