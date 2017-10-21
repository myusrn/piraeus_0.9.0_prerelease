
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

namespace Piraeus.Adapters
{
    public abstract class ProtocolAdapterFactory
    {
        /// <summary>
        /// Creates a protocol adapter for TCP server channel
        /// </summary>
        /// <param name="client">TCP client initialized by TCP Listener on server.</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, TcpClient client, CancellationToken token)
        {
            IChannel channel = ChannelFactory.Create(client, config.Channels.Tcp.BlockSize, config.Channels.Tcp.MaxBufferSize,token);
            IPEndPoint localEP = (IPEndPoint)client.Client.LocalEndPoint;
            int port = localEP.Port;

            if(port == 5684) //CoAP over TCP
            {

                return new CoapProtocolAdapter(channel, null);
            }
            else if (port == 1883 || port == 8883) //MQTT over TCP
            {
                //MQTT
                return null;
            }
            else
            {
                throw new ProtocolAdapterPortException("TcpClient port does not map to a supported protocol.");
            }           

        }

        
    }
}
