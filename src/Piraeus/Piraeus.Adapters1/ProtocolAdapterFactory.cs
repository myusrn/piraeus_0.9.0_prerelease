
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Authentication;
using System.Net.Http;
using System.Web;
using SkunkLab.Channels.WebSocket;

namespace Piraeus.Adapters
{
    public abstract class ProtocolAdapterFactory
    {

        private static CoapConfig coapConfig;
        private static MqttConfig mqttConfig;


        /// <summary>
        /// Create protocol adapter for rest service or Web socket
        /// </summary>
        /// <param name="config"></param>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <param name="authenticator"></param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, HttpRequestMessage request, CancellationToken token, IAuthenticator authenticator = null)
        {
            IChannel channel = null;

            coapConfig = new CoapConfig(authenticator, config.Protocols.Coap.HostName, CoapConfigOptions.NoResponse | CoapConfigOptions.Observe);
            mqttConfig = new MqttConfig(authenticator);

            HttpContext context = HttpContext.Current;
            if (context.IsWebSocketRequest ||
                context.IsWebSocketRequestUpgrading)
            {
                WebSocketConfig webSocketConfig = GetWebSocketConfig(config);
                channel = ChannelFactory.Create(request, webSocketConfig, token);

                if (context.WebSocketRequestedProtocols.Contains("mqtt"))
                {
                    return new MqttProtocolAdapter(mqttConfig, channel);
                }
                else if (context.WebSocketRequestedProtocols.Contains("coapv1"))
                {
                    return new CoapProtocolAdapter(coapConfig, channel);
                }
                else if (context.WebSocketRequestedProtocols.Count == 0)
                {
                    //wsn protocol
                    return new WsnProtocolAdapter(config, channel);
                }
                else
                {
                    throw new InvalidOperationException("invalid web socket subprotocol");
                }
            }

            if (request.Method != HttpMethod.Post && request.Method != HttpMethod.Get)
            {
                throw new HttpRequestException("Protocol adapter requires HTTP get or post.");
            }
            else
            {
                channel = ChannelFactory.Create(request);
                return new RestProtocolAdapter(config, channel);
            }
        }

        /// <summary>
        /// Creates a protocol adapter for TCP server channel
        /// </summary>
        /// <param name="client">TCP client initialized by TCP Listener on server.</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, IAuthenticator authenticator, TcpClient client, CancellationToken token)
        {
            IChannel channel = ChannelFactory.Create(client, config.Channels.Tcp.BlockSize, config.Channels.Tcp.MaxBufferSize, token);
            IPEndPoint localEP = (IPEndPoint)client.Client.LocalEndPoint;
            int port = localEP.Port;

            if (port == 5684) //CoAP over TCP
            {
                return new CoapProtocolAdapter(GetCoapConfig(config, authenticator), channel);
            }
            else if (port == 1883 || port == 8883) //MQTT over TCP
            {
                //MQTT
                return new MqttProtocolAdapter(GetMqttConfig(config, authenticator), channel);
            }
            else
            {
                throw new ProtocolAdapterPortException("TcpClient port does not map to a supported protocol.");
            }

        }
    

        #region configurations
        private static WebSocketConfig GetWebSocketConfig(PiraeusConfig config)
        {
            return new WebSocketConfig(config.Channels.WebSocket.MaxIncomingMessageSize,
                config.Channels.WebSocket.ReceiveLoopBufferSize,
                config.Channels.WebSocket.SendBufferSize,
                config.Channels.WebSocket.CloseTimeoutMilliseconds);
        }

        private static CoapConfig GetCoapConfig(PiraeusConfig config, IAuthenticator authenticator)
        {
            if (coapConfig == null)
            {
                CoapConfigOptions options = config.Protocols.Coap.ObserveOption && config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.Protocols.Coap.ObserveOption ? CoapConfigOptions.Observe : config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
                coapConfig = new CoapConfig(authenticator, config.Protocols.Coap.HostName, options, config.Protocols.Coap.AutoRetry,
                    config.Protocols.Coap.KeepAliveSeconds, config.Protocols.Coap.AckTimeoutSeconds, config.Protocols.Coap.AckRandomFactor,
                    config.Protocols.Coap.MaxRetransmit, config.Protocols.Coap.NStart, config.Protocols.Coap.DefaultLeisure, config.Protocols.Coap.ProbingRate, config.Protocols.Coap.MaxLatencySeconds);
            }

            return coapConfig;
        }

        private static MqttConfig GetMqttConfig(PiraeusConfig config, IAuthenticator authenticator)
        {
            if (mqttConfig == null)
            {
                mqttConfig = new MqttConfig(authenticator, config.Protocols.Mqtt.KeepAliveSeconds,
                   config.Protocols.Mqtt.AckTimeoutSeconds, config.Protocols.Mqtt.AckRandomFactor, config.Protocols.Mqtt.MaxRetransmit, config.Protocols.Mqtt.MaxLatencySeconds);
            }

            return mqttConfig;
        }

        #endregion
    }
}
