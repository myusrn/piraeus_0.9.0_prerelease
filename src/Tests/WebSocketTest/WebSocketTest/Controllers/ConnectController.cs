using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SkunkLab.Channels.WebSocket;
using System.Threading;
using System.Diagnostics;

namespace WebSocketTest.Controllers
{
    public class ConnectController : ApiController
    {
        private WebSocketHandler handler;
        private CancellationTokenSource source;
        private WebSocketServerChannel channel;

        [HttpGet]
        public HttpResponseMessage Get()
        {
            source = new CancellationTokenSource();
            HttpContext context = HttpContext.Current;

            if (context.IsWebSocketRequest ||
            context.IsWebSocketRequestUpgrading)
            {
                WebSocketConfig config = new WebSocketConfig();
                //handler = new WebSocketHandler(config, source.Token);

                channel = new WebSocketServerChannel(Request, config, source.Token);
                channel.OnClose += Channel_OnClose;
                channel.OnError += Channel_OnError;
                channel.OnOpen += Channel_OnOpen;
                channel.OnReceive += Channel_OnReceive;

                //context.AcceptWebSocketRequest(channel);

                //handler.OnOpen += Handler_OnOpen;
                //handler.OnReceive += Handler_OnReceive;
                //handler.OnError += Handler_OnError;
                //handler.OnClose += Handler_OnClose;
                //context.AcceptWebSocketRequest(handler);

                return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

        }

        private void Channel_OnReceive(object sender, SkunkLab.Channels.ChannelReceivedEventArgs e)
        {
            //Console.WriteLine("Channel received {0}", e.Message.Length);
            Debug.WriteLine(String.Format("Server channel received {0}", e.Message.Length));
            Task t = channel.SendAsync(e.Message);
            Task.WhenAll(t);
        }

        private void Channel_OnOpen(object sender, SkunkLab.Channels.ChannelOpenEventArgs e)
        {
            Console.WriteLine("Channel open");
        }

        private void Channel_OnError(object sender, SkunkLab.Channels.ChannelErrorEventArgs e)
        {
            Console.WriteLine("Channel error {0}", e.Error.Message);
        }

        private void Channel_OnClose(object sender, SkunkLab.Channels.ChannelCloseEventArgs e)
        {
            Console.WriteLine("Channel closed");
        }

        private void Handler_OnClose(object sender, WebSocketCloseEventArgs args)
        {
            Console.WriteLine("Closing");
        }

        private void Handler_OnError(object sender, WebSocketErrorEventArgs args)
        {
            Console.WriteLine("Error {0}", args.Error.Message);
        }

        private void Handler_OnReceive(object sender, WebSocketReceiveEventArgs args)
        {
            Console.WriteLine("Received {0}", args.Message.Length);
        }

        private void Handler_OnOpen(object sender, WebSocketOpenEventArgs args)
        {
            Console.WriteLine("Opened");
        }
    }
}
