using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Channels.Http;
using SkunkLab.Channels.WebSocket;

namespace SkunkLab.Channels
{
    public abstract class ChannelFactory
    {
        #region TCP Channels

        #region TCP Server Channels


        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, CancellationToken token)
        {
            return TcpChannel.Create(client, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="certificate"></param>
        /// <param name="clientAuth"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, X509Certificate2 certificate, bool clientAuth, CancellationToken token)
        {
            return TcpChannel.Create(client, certificate, clientAuth, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, string pskIdentity, byte[] psk, CancellationToken token)
        {
            return TcpChannel.Create(client, pskIdentity, psk, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(client, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="certificate"></param>
        /// <param name="clientAuth"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, X509Certificate2 certificate, bool clientAuth, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(client, certificate, clientAuth, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(client, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        #endregion

        #region TCP Client Channels
        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, IPEndPoint localEP, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, localEP, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, localEP, token);
        }



        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, CancellationToken token)
        {
            return TcpChannel.Create(address, port, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, localEP, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, localEP, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(address, port, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, certificate, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, pskIdentity, psk, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, localEP, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, localEP, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(address, port, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(hostname, port, localEP, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(remoteEndpoint, localEP, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(address, port, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        #endregion
        #endregion

        #region HTTP Channels

        #region HTTP Server Channels

        /// <summary>
        /// HTTP server channel used to receive messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IChannel Create(HttpRequestMessage request)
        {
            return HttpChannel.Create(request);
        }

        /// <summary>
        /// HTTP server channel used to transmit messages to endpoints.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string resourceUriString, string contentType)
        {
            return HttpChannel.Create(endpoint, resourceUriString, contentType);
        }

        /// <summary>
        /// HTTP server channel used to transmit messages to endpoints.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="securityToken"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            return HttpChannel.Create(endpoint, resourceUriString, contentType, securityToken);
        }

        #endregion

        #region HTTP Client Channels
        /// <summary>
        /// HTTP client channel used to transmit and receive via long polling messages from server.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate)
        {
            return HttpChannel.Create(endpoint, resourceUriString, contentType, certificate);
        }

        /// <summary>
        /// HTTP client channel used to transmit and receive via long polling messages from server.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="securityToken"></param>
        /// <param name="contentType"></param>
        /// <param name="observers"></param>
        /// <param name="indexes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri requestUri, string securityToken, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            return HttpChannel.Create(requestUri, securityToken, contentType, observers, indexes, token);
        }

        /// <summary>
        /// HTTP client channel used to transmit and receive via long polling messages from server.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="certificate"></param>
        /// <param name="contentType"></param>
        /// <param name="observers"></param>
        /// <param name="indexes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri requestUri, X509Certificate2 certificate, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            return HttpChannel.Create(requestUri, certificate, contentType, observers, indexes, token);
        }

        /// <summary>
        /// HTTP client channel used to transmit and receive via long polling messages from server.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="contentType"></param>
        /// <param name="observers"></param>
        /// <param name="indexes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri requestUri, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            return HttpChannel.Create(requestUri, contentType, observers, indexes, token);
        }

        #endregion

        #endregion

        #region Web Socket Channels

        #region Web Socket Server Channels

        /// <summary>
        /// Create Web socket server channel.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(HttpRequestMessage request, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(request, config, token);
        }

        #endregion

        #region Web Socket Client Channels

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, config, token);
        }

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, subProtocol, config, token);
        }

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="securityToken"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, string securityToken, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, securityToken, subProtocol, config, token);
        }


        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="certificate"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, X509Certificate2 certificate, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, certificate, subProtocol, config, token);
        }
        #endregion
        #endregion

        #region UDP Channels

        #region UDP Server Channels

        /// <summary>
        /// Creates UDP server channel
        /// </summary>
        /// <param name="localAddress"></param>
        /// <param name="localPort"></param>
        /// <param name="remoteAddress"></param>
        /// <param name="remotePort"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(IPAddress localAddress, int localPort, IPAddress remoteAddress, int remotePort, CancellationToken token)
        {
            IPEndPoint localEP = new IPEndPoint(localAddress, localPort);
            IPEndPoint remoteEP = new IPEndPoint(remoteAddress, remotePort);
            return UdpChannel.Create(localEP, remoteEP, token);
        }

        #endregion

        #region UDP Client Channels

        /// <summary>
        /// Creates UDP client channel
        /// </summary>
        /// <param name="localEP"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint localEP, string hostname, int port, CancellationToken token)
        {
            return UdpChannel.Create(localEP, hostname, port, token);
        }

        /// <summary>
        /// Creates UDP client channel
        /// </summary>
        /// <param name="localEP"></param>
        /// <param name="remoteAddress"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(IPEndPoint localEP, IPAddress remoteAddress, int port, CancellationToken token)
        {
            return UdpChannel.Create(localEP, remoteAddress, port, token);
        }

        #endregion

        #endregion

    }
}
