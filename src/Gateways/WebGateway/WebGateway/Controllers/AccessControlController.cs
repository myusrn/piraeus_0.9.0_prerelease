using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Capl.Authorization;
using Piraeus.Grains;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class AccessControlController : ApiController
    {
        public AccessControlController()
        {
            bool started = OrleansClientConfig.TryStart("AccessControlController");
            
        }

        [CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAccessControlPolicy(string policyUriString)
        {
            try
            {
                AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(policyUriString);
                return Request.CreateResponse<AuthorizationPolicy>(HttpStatusCode.OK, policy, "application/xml");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpsertAccessControlPolicy(AuthorizationPolicy policy)
        {
            try
            {
                await GraphManager.UpsertAcessControlPolicyAsync(policy.PolicyId.ToString(), policy);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAccessControlPolicy(string policyUriString)
        {
            try
            {
                await GraphManager.ClearAccessControlPolicyAsync(policyUriString);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
