using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class ResourceController : ApiController
    {
        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetResourceList()
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetResourceListAsync();
                return Request.CreateResponse<IEnumerable<string>>(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetResourceMetadata(string resourceUriString)
        {
            try
            {
                ResourceMetadata metadata = await GraphManager.GetResourceMetadata(resourceUriString);
                return Request.CreateResponse<ResourceMetadata>(HttpStatusCode.OK, metadata);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpsertResourceMetadata(ResourceMetadata metadata)
        {
            try
            {
                await GraphManager.UpsertResourceMetadataAsync(metadata);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpPost]
        public async Task<HttpResponseMessage> Subscribe(string resourceUriString, SubscriptionMetadata metadata)
        {
            try
            {
                string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);
                return Request.CreateResponse<string>(HttpStatusCode.OK, subscriptionUriString);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetResourceSubscriptionList(string resourceUriString)
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetResourceSubscriptionListAsync(resourceUriString);
                return Request.CreateResponse<IEnumerable<string>>(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpDelete]
        public async Task<HttpResponseMessage> Unsubscribe(string subscriptionUriString)
        {
            try
            {
                await GraphManager.UnsubscribeAsync(subscriptionUriString.ToLowerInvariant());
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteResource(string resourceUriString)
        {
            try
            {
                await GraphManager.ClearResourceAsync(resourceUriString);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
