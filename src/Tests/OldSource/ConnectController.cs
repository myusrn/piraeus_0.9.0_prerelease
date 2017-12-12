using Orleans.Runtime.Host;
using Piraeus.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PiraeusGatewayWebRole.Controllers
{
    public class ConnectController : ApiController
    {
        private ProtocolAdapter adapter;
        public ConnectController()
        {
            if (!AzureClient.IsInitialized)
            {
                FileInfo clientConfigFile = AzureConfigUtils.ClientConfigFileLocation;
                if (!clientConfigFile.Exists)
                {
                    throw new FileNotFoundException(string.Format("Cannot find Orleans client config file for initialization at {0}", clientConfigFile.FullName), clientConfigFile.FullName);
                }
                AzureClient.Initialize(clientConfigFile);
            }
        }



        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {

            try
            {
                this.adapter = ProtocolAdapter.Create(Request);
                this.adapter.Initialize();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (AggregateException ae)
            {
                Trace.TraceError(ae.InnerException.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.InnerException.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }


        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                this.adapter = ProtocolAdapter.Create(Request);
                this.adapter.Initialize();
                return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            }
            catch (AggregateException ae)
            {
                Trace.TraceError(ae.InnerException.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.InnerException.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}
