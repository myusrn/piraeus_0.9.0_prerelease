using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class SubscriptionController : ApiController
    {
        public SubscriptionController()
        {
            bool started = OrleansClientConfig.TryStart("SubscriptionController");
            
        }

        [CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
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

        [HttpGet]
        public async Task<HttpResponseMessage> GetSubscriptionMetrics(string subscriptionUriString)
        {
            try
            {
                CommunicationMetrics metrics = await GraphManager.GetSubscriptionMetricsAsync(subscriptionUriString);
                return Request.CreateResponse<CommunicationMetrics>(HttpStatusCode.OK, metrics);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
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
