using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Orleans;
using Orleans.Runtime.Configuration;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Configuration.Settings;

namespace WebGateway.Controllers
{
    public class ConnectController : ApiController
    {
        private ProtocolAdapter adapter;
        private PiraeusConfig config;
        private CancellationTokenSource source;
        private delegate HttpResponseMessage HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);


        public ConnectController()
        {
            config = PiraeusConfigManager.Settings;
            source = new CancellationTokenSource();
            OnMessage += ConnectController_OnMessage;
            //ClientConfiguration clientConfig = ClientConfiguration.LocalhostSilo();

            //try
            //{
            //    var config = ClientConfiguration.LocalhostSilo();
                
            //    GrainClient.Initialize(config);
            //    //ClientConfiguration clientConfig = ClientConfiguration.LoadFromFile("C:\\_git\\core\\Tests\\ProtocolAdapters\\Web\\WebGateway\\ClientConfiguration.xml");
            //    //IClusterClient client = new ClientBuilder().UseConfiguration(clientConfig).Build();
            //    //Task task = client.Connect();
            //    //Task.WaitAll(task);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        [HttpPost]
        public HttpResponseMessage Post()
        {
            adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        

        [HttpGet]
        public HttpResponseMessage Get()
        {
            adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
            adapter.OnObserve += Adapter_OnObserve;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();


            if (HttpContext.Current.IsWebSocketRequest || HttpContext.Current.IsWebSocketRequestUpgrading)
            {
                //web socket channel
                return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            }
            else
            {
                HttpResponseMessage message = null;
                OnMessage += (obj, args) => { message = Request.CreateResponse(HttpStatusCode.OK, args.Message, args.ContentType);  return message; };
                return message;
            }
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            source.Cancel();
            adapter.Dispose();
        }


        private HttpResponseMessage ConnectController_OnMessage(object sender, SkunkLab.Channels.ChannelObserverEventArgs args)
        {
            return Request.CreateResponse();
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
    }
}
