using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace Piraeus.Clients.Coap
{
    public class PiraeusCoapClient
    {
        public PiraeusCoapClient(CoapConfig config, IChannel channel, ICoapRequestDispatch dispatcher = null)
        {

            this.config = config;
            this.pingId = new List<ushort>();
            session = new CoapSession(config);
            
            observers = new Dictionary<string, string>();
            this.dispatcher = dispatcher;
            this.channel = channel;
            this.channel.OnClose += Channel_OnClose;
            this.channel.OnError += Channel_OnError;
            this.channel.OnOpen += Channel_OnOpen;
            this.channel.OnStateChange += Channel_OnStateChange;
            this.channel.OnReceive += Channel_OnReceive;
            session.OnRetry += Session_OnRetry;
            session.OnKeepAlive += Session_OnKeepAlive;
        }

        
        private Dictionary<string, string> observers;
        private IChannel channel;
        private CoapConfig config;
        private CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private List<ushort> pingId;

        public event System.EventHandler<CoapMessageEventArgs> OnPingResponse;
        
        public async Task PublishAsync(string resourceUriString, string contentType, byte[] payload, bool confirmable, Action<CodeType, string, byte[]> action)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token, null, action);            

            RequestMessageType mtype = confirmable ? RequestMessageType.Confirmable : RequestMessageType.NonConfirmable;
            CoapRequest cr = new CoapRequest(id, mtype, MethodType.POST, new Uri(resourceUriString), MediaTypeConverter.ConvertToMediaType(contentType), payload);
            await channel.SendAsync(cr.Encode());
        }

        

        public async Task PublishAsync(string resourceUriString, string contentType, byte[] payload, NoResponseType nrt)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token);
            CoapRequest cr = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.POST, new Uri(resourceUriString), MediaTypeConverter.ConvertToMediaType(contentType), payload);
            cr.NoResponse = nrt;
            await channel.SendAsync(cr.Encode());
        }

        public async Task SubscribeAsync(string resourceUriString, bool confirmable, Action<CodeType, string, byte[]> action)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token, null, action);
            RequestMessageType mtype = confirmable ? RequestMessageType.Confirmable : RequestMessageType.NonConfirmable;
            CoapRequest cr = new CoapRequest(id, mtype, MethodType.PUT, token, new Uri(resourceUriString), null);
            await channel.SendAsync(cr.Encode());
        }

        public async Task SubscribeAsync(string resourceUriString, NoResponseType nrt)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token);
            CoapRequest cr = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.PUT, token, new Uri(resourceUriString), null);
            cr.NoResponse = nrt;
            await channel.SendAsync(cr.Encode());
        }

        public async Task UnsubscribeAsync(string resourceUriString, bool confirmable, Action<CodeType, string, byte[]> action)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token, null, action);
            RequestMessageType mtype = confirmable ? RequestMessageType.Confirmable : RequestMessageType.NonConfirmable;
            CoapRequest cr = new CoapRequest(id, mtype, MethodType.DELETE, token, new Uri(resourceUriString), null);
            await channel.SendAsync(cr.Encode());
        }

        public async Task UnsubscribeAsync(string resourceUriString, NoResponseType nrt)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token);
            CoapRequest cr = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.DELETE, token, new Uri(resourceUriString), null);
            cr.NoResponse = nrt;
            await channel.SendAsync(cr.Encode());
        }

        public async Task ObserveAsync(string resourceUriString, Action<CodeType, string, byte[]> action)
        {
            session.UpdateKeepAliveTimestamp();

            byte[] token = CoapToken.Create().TokenBytes;
            ushort id = session.CoapSender.NewId(token, true, action);
            string scheme = channel.IsEncrypted ? "coaps" : "coap";
            string coapUriString = String.Format("{0}://{1}?r={2}", scheme, config.Authority, resourceUriString);
            CoapRequest cr = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.GET, token, new Uri(coapUriString), null);
            cr.Observe = true;
            observers.Add(resourceUriString, Convert.ToBase64String(token));
            await channel.SendAsync(cr.Encode());
        }

        public async Task UnobserveAsync(string resourceUriString)
        {
            session.UpdateKeepAliveTimestamp();

            if(observers.ContainsKey(resourceUriString))
            {
                string tokenString = observers[resourceUriString];
                byte[] token = Convert.FromBase64String(tokenString);
                ushort id = session.CoapSender.NewId(token, false, null);
                string scheme = channel.IsEncrypted ? "coaps" : "coap";
                string coapUriString = String.Format("{0}://{1}?r={2}", scheme, config.Authority, resourceUriString);

                CoapRequest request = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.GET, new Uri(coapUriString), null);
                request.Observe = false;
                await channel.SendAsync(request.Encode());

                session.CoapSender.Unobserve(Convert.FromBase64String(tokenString));
            }

            
        }

        #region channel events
        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            CoapMessage message = CoapMessage.DecodeMessage(args.Message);
            CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);
            Task<CoapMessage> task = handler.ProcessAsync();
            Task<CoapMessage>.WhenAll(task);
            CoapMessage msg = task.Result;
            if(msg != null && pingId.Contains(msg.MessageId))
            {
                pingId.Remove(msg.MessageId);
                //ping complete
                OnPingResponse?.Invoke(this, new CoapMessageEventArgs(msg));
            }
            
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion


        private void Session_OnRetry(object sender, CoapMessageEventArgs args)
        {
            pingId.Add(args.Message.MessageId);
            Task task = channel.SendAsync(args.Message.Encode());
            Task.WhenAll(task);
        }

        private void Session_OnKeepAlive(object sender, CoapMessageEventArgs args)
        {
            Task task = channel.SendAsync(args.Message.Encode());
            Task.WhenAll(task);
        }


    }
}
