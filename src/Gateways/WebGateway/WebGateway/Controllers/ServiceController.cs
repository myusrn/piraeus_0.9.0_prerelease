using Piraeus.Grains;
using System;
using System.Collections.Generic;
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
            bool started = OrleansClientConfig.TryStart("ServiceController");
        }
        [HttpPost]
        public async Task<HttpResponseMessage> ConfigureIdentity(List<Claim> claims = null, string base64Certificate = null)
        {
            try
            {
                X509Certificate2 certificate = null;

                if (base64Certificate != null)
                {
                    byte[] certBytes = Convert.FromBase64String(base64Certificate);
                    certificate = new X509Certificate2(certBytes);
                }

                await GraphManager.SetServiceIdentityAsync(claims, certificate);
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
