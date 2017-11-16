using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Piraeus.Core.Metadata;
using Piraeus.Grains;

namespace WebGatewayTest.Controllers
{
    public class SubscriptionController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> GetSubscriptionMetadata(string subscriptionUriString)
        {
            try
            {
                SubscriptionMetadata metadata = await GraphManager.GetSubscriptionMetadataAsync(subscriptionUriString);
                return Request.CreateResponse<SubscriptionMetadata>(HttpStatusCode.OK, metadata);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpsertSubscriptionMetadata(SubscriptionMetadata metadata)
        {
            try
            {
                await GraphManager.UpsertSubscriptionMetadataAsync(metadata);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
