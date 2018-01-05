using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using SkunkLab.Security.Authentication;
using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {  
        public CoapProtocolAdapter(PiraeusConfig config, IAuthenticator authenticator, IChannel channel)
        {

            CoapConfigOptions options = config.Protocols.Coap.ObserveOption && config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.Protocols.Coap.ObserveOption ? CoapConfigOptions.Observe : config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
            CoapConfig coapConfig = new CoapConfig(authenticator, config.Protocols.Coap.HostName, options, config.Protocols.Coap.AutoRetry,
                config.Protocols.Coap.KeepAliveSeconds, config.Protocols.Coap.AckTimeoutSeconds, config.Protocols.Coap.AckRandomFactor,
                config.Protocols.Coap.MaxRetransmit, config.Protocols.Coap.NStart, config.Protocols.Coap.DefaultLeisure, config.Protocols.Coap.ProbingRate, config.Protocols.Coap.MaxLatencySeconds);


            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnSent += Channel_OnSent;
            Channel.OnStateChange += Channel_OnStateChange;
            session = new CoapSession(coapConfig);            
        }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;
        private CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private bool disposedValue;
        private bool forcePerReceiveAuthn;
        


        public override IChannel Channel { get; set; }


        public override void Init()
        {
            forcePerReceiveAuthn = Channel as UdpChannel != null;

            dispatcher = new CoapRequestDispatcher(session, Channel);
            Task task = Channel.OpenAsync();
            Task.WaitAll(task);
        }

     


        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dispatcher.Dispose();
                    session.Dispose();
                    Channel.Dispose();

                }

                disposedValue = true;
            }
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Channel Events
        

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
           
            //opened = true;
            Task task = Log.LogInfoAsync("Channel {0} opened.", e.ChannelId);
            Task.WhenAll(task);

            session.IsAuthenticated = Channel.IsAuthenticated;

            try
            {
                if (!Channel.IsAuthenticated && e.Message != null)
                {
                    CoapMessage msg = CoapMessage.DecodeMessage(e.Message);
                    CoapUri coapUri = new CoapUri(msg.ResourceUri.ToString());
                    session.IsAuthenticated = session.Authenticate(coapUri.TokenType, coapUri.SecurityToken);
                }
            }
            catch (Exception ex)
            {
                Task t = Log.LogErrorAsync("Channel {0} not authenticated. Exception {1}", Channel.Id, ex.Message);
                Task.WhenAll(t);
            }

            if (!session.IsAuthenticated && e.Message != null)
            {
                //close the channel
                Task logTask = Log.LogErrorAsync("Channel {0} not authenticated. Closing channel.", Channel.Id);
                Task.WhenAll(logTask);

                Task closeTask = Channel.CloseAsync();
                Task.WaitAll(closeTask);
            }

        }
        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Task logTask = Log.LogInfoAsync("Channel {0} received message.", e.ChannelId);
            Task.WhenAll(logTask);

            CoapMessage message = CoapMessage.DecodeMessage(e.Message);     
            
            if(!session.IsAuthenticated || forcePerReceiveAuthn)
            {
                try
                {
                    CoapAuthentication.EnsureAuthentication(session, message, forcePerReceiveAuthn);
                }
                catch
                {
                    Task t = Channel.CloseAsync();
                    Task.WhenAll(t);
                    return;
                }
            }
            


            OnObserve?.Invoke(this, new ChannelObserverEventArgs(message.ResourceUri.ToString(), MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload));
                        
            Task task = Forward(message);
            Task.WhenAll(task);
            //Task task = Forward(message);
            //Task.WhenAll(task);


            //CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);


            //CoapMessage  msg = handler.ProcessAsync().GetAwaiter().GetResult();
            //if(msg != null)
            //{
            //    Channel.SendAsync(msg.Encode()).GetAwaiter().GetResult();
            //}

            
            //Task t = Task.Factory.StartNew(async () =>
            //{
            //    CoapMessage msg = await handler.ProcessAsync();
            //    if (msg != null)
            //    {
            //        await Channel.SendAsync(msg.Encode());
            //    }
            //});
            //Task.WhenAll(t);
        }


        private async Task Forward(CoapMessage message)
        {
            CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);

            await Log.LogInfoAsync("Coap handler about to start processing");
            CoapMessage msg = await handler.ProcessAsync();
            await Log.LogInfoAsync("Coap handler processed.");

            if (msg != null)
            {
                await Log.LogInfoAsync("Coap handler message to be sent.");
                await Channel.SendAsync(msg.Encode());
                await Log.LogInfoAsync("Coap handler message sent.");

            }
            else
            {
                await Log.LogInfoAsync("Coap handler return null messsage.");
            }

        }



        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));

            Task task = Log.LogErrorAsync("Channel ID {0} with error {1}", Channel.Id, e.Error.Message);
            Task.WhenAll(task);

            Task closeTask = Channel.CloseAsync();
            Task.WhenAll(closeTask);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
            //Channel.Dispose();

            Task task = Log.LogInfoAsync("Channel {0} closing.", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} state {1}", Channel.Id, e.State);
            Task.WhenAll(task);
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} send message", Channel.Id);
            Task.WhenAll(task);
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs e)
        {
            Task task = Log.LogInfoAsync("Channel {0} retrying message", Channel.Id);
            Task.WhenAll(task);
        }
        #endregion
    }
}
