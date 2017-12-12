using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Piraeus.Adapters;
using System.Web;
using System.Threading;
using Piraeus.Core.Utilities;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using SkunkLab.Channels;
using Piraeus.Configuration.Settings;

namespace WebGateway.Controllers
{
    public class ConnectController : ApiController
    {
        public ConnectController()
        {
            config = Piraeus.Configuration.PiraeusConfigManager.Settings;
            source = new CancellationTokenSource();
        }

        private WebSocketHandler handler;
        private CancellationTokenSource source;
        private Piraeus.Configuration.Settings.PiraeusConfig  config;
        private delegate void HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;
        private ProtocolAdapter adapter;
        private HttpResponseMessage response;
        private WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false)
        };

        [HttpPost]
        public HttpResponseMessage Post()
        {
            try
            {
                adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
                adapter.OnClose += Adapter_OnClose;
                adapter.Init();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }        

        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                //MessageUri mu = new MessageUri(Request);
                SkunkLab.Security.Authentication.BasicAuthenticator authn = new SkunkLab.Security.Authentication.BasicAuthenticator();
                //adapter = ProtocolAdapterFactory.Create(config, Request, source.Token, authn);
                
                //adapter.OnClose += Adapter_OnClose;
                //adapter.OnError += Adapter_OnError;
                

                HttpContext context = HttpContext.Current;

                if (context.IsWebSocketRequest ||
                context.IsWebSocketRequestUpgrading)
                {
                    //WebSocketConfig config = new WebSocketConfig();
                    //handler = new WebSocketHandler(config, source.Token);
                    //HttpContext.Current.AcceptWebSocketRequest(handler);
                   // WebSocketConfig wsc = new WebSocketConfig();
                   // WebSocketServerChannel wschannel = new WebSocketServerChannel(Request, wsc, source.Token);
                    //CoapConfig cc = new CoapConfig(authn, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe);

                    //IChannel channel = ChannelFactory.Create(Request, new WebSocketConfig(), source.Token);

                    PiraeusConfig config = Piraeus.Configuration.PiraeusConfigManager.Settings;

                    adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
                }
                else //long polling
                {
                    //adapter.OnObserve += Adapter_OnObserve;
                    //adapter.Init();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Listen), waitHandles[0]);
                    WaitHandle.WaitAll(waitHandles);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Exception ex = e.Error;
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            //source.Cancel();
            adapter.Dispose();
        }

        private void Listen(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            OnMessage += (o, a) => {
                response = Request.CreateResponse(HttpStatusCode.OK, a.Message, a.ContentType);
                response.Headers.Add("x-sl-resource", a.ResourceUriString);
                are.Set();
            };
        }
    }
}
