using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SkunkLab.Security.Tokens;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public delegate void EventHandler<CoapMessageEventArgs>(object sender, CoapMessageEventArgs args);
    public delegate CoapMessage RespondingEventHandler(object sender, CoapMessageEventArgs args);
    public class CoapSession
    {
        public CoapSession(CoapConfig config)
        {
            Config = config;
            
            CoapReceiver = new Receiver(config.ExchangeLifetime.TotalMilliseconds);
            CoapSender = new Transmitter(config.ExchangeLifetime.TotalMilliseconds, config.MaxTransmitSpan.TotalMilliseconds, config.MaxRetransmit);
            CoapSender.OnRetry += Transmit_OnRetry;

            if(config.KeepAlive.HasValue)
            {
                keepaliveTimestamp = DateTime.UtcNow.AddMilliseconds(config.KeepAlive.Value);
                keepaliveTimer = new Timer(config.KeepAlive.Value);
                keepaliveTimer.Elapsed += KeepaliveTimer_Elapsed;
                keepaliveTimer.Start();
            }
        }

        

        public event EventHandler<CoapMessageEventArgs> OnRetry;
        public event EventHandler<CoapMessageEventArgs> OnKeepAlive;

        public bool IsAuthenticated { get; set; }
        public Transmitter CoapSender { get; internal set; }

        public Receiver CoapReceiver { get; internal set; }
                
        

        public CoapConfig Config { get; internal set; }

        private DateTime keepaliveTimestamp;
        private Timer keepaliveTimer;

        public bool Authenticate(string tokenType, string token)
        {
            SecurityTokenType tt = (SecurityTokenType)Enum.Parse(typeof(SecurityTokenType), tokenType, true);
            IsAuthenticated = Config.Authenticator.Authenticate(tt, token);
            return IsAuthenticated;
        }


        public bool IsNoResponse(NoResponseType? messageNrt, NoResponseType result)
        {
            
            if(!messageNrt.HasValue)
            {
                return false;
            }

            return messageNrt.Value.HasFlag(result);
        }

        public bool CanObserve()
        {
            return Config.ConfigOptions.HasFlag(CoapConfigOptions.Observe);
        }

        public void UpdateKeepAliveTimestamp()
        {
            keepaliveTimestamp = DateTime.UtcNow.AddMilliseconds(Config.KeepAlive.Value);
        }

        private void KeepaliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (keepaliveTimestamp <= DateTime.UtcNow)
            {
                //signal a ping
                CoapToken token = CoapToken.Create();
                ushort id = CoapSender.NewId(token.TokenBytes);
                CoapRequest ping = new CoapRequest()
                {
                    MessageId = id,
                    Token = token.TokenBytes,
                    Code = CodeType.EmptyMessage,
                    MessageType = CoapMessageType.Confirmable
                };

                OnKeepAlive?.Invoke(this, new CoapMessageEventArgs(ping));
            }
        }

        private void Transmit_OnRetry(object sender, CoapMessageEventArgs e)
        {
            OnRetry?.Invoke(this, e);
        }

       

    }
}
