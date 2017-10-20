using SkunkLab.Channels.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public abstract class TcpChannel : IChannel
    {
        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(TcpClient client, CancellationToken token)
        {
            return new TcpServerChannel(client, token);
        }

        public static TcpChannel Create(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, CancellationToken token)
        {
            return new TcpClientChannel(address, port, localEP, pskIdentity, psk, token);
        }

        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="certificate">Server certificate used for authentication.</param>
        /// <param name="clientAuth">Determines whether to authenticate the client certificate.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(TcpClient client, X509Certificate2 certificate, bool clientAuth, CancellationToken token)
        {
            return new TcpServerChannel(client, certificate, clientAuth, token);
        }

        /// <summary>
        /// Create new TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(TcpClient client, string pskIdentity, byte[] psk, CancellationToken token)
        {
            return new TcpServerChannel(client, pskIdentity, psk, token);
        }

        /// <summary>
        /// Create new TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(TcpClient client, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpServerChannel2(client, blockSize, maxBufferSize, token);
        }

        

        /// <summary>
        /// Creates new TCP server channel
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
            return new TcpServerChannel2(client, certificate, clientAuth, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Create new TCP server channel
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
            return new TcpServerChannel2(client, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Create a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint to bind for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, CancellationToken token)
        {
            return new TcpClientChannel(address, port, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(address, port, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, certificate, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, localEP, certificate, token);
        }

        

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, certificate, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, localEP, certificate, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate to authenticate client.</param>
        /// <param name="token">Cancellation token.</param>
        public static TcpChannel Create(IPAddress address, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(address, port, certificate, token);
        }

        /// <summary>
        /// Create new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(address, port, localEP, certificate, token);
        }

        public static IChannel Create(string hostname, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(hostname, port, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(string hostname, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(hostname, port, localEP, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPEndPoint remoteEndpoint, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(remoteEndpoint, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(remoteEndpoint, localEP, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPAddress address, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(address, port, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(address, port, localEP, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(string hostname, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(hostname, port, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(hostname, port, localEP, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPEndPoint remoteEndpoint, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(remoteEndpoint, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(remoteEndpoint, localEP, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPAddress address, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(address, port, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(address, port, localEP, certificate, blockSize, maxBufferSize, token);
        }

        public static IChannel Create(IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return new TcpClientChannel2(address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }


        public abstract int Port { get; internal set; }
        public abstract bool IsConnected { get; }
        public abstract string Id { get; internal set; }

        public abstract bool IsEncrypted { get; internal set; }

        public abstract bool IsAuthenticated { get; internal set; }

        public abstract ChannelState State { get; internal set; }

        public abstract event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public abstract event EventHandler<ChannelCloseEventArgs> OnClose;
        public abstract event EventHandler<ChannelOpenEventArgs> OnOpen;
        public abstract event EventHandler<ChannelErrorEventArgs> OnError;
        public abstract event EventHandler<ChannelStateEventArgs> OnStateChange;
        public abstract event EventHandler<ChannelRetryEventArgs> OnRetry;
        public abstract event EventHandler<ChannelSentEventArgs> OnSent;
        public abstract event EventHandler<ChannelObserverEventArgs> OnObserve;

        //public abstract event ChannelReceivedEventHandler OnReceive;
        //public abstract event ChannelCloseEventHandler OnClose;
        //public abstract event ChannelOpenEventHandler OnOpen;
        //public abstract event ChannelErrorEventHandler OnError;
        //public abstract event ChannelStateEventHandler OnStateChange;
        //public abstract event ChannelRetryEventHandler OnRetry;
        //public abstract event ChannelSentEventHandler OnSent;

        public abstract Task CloseAsync();

        public abstract void Dispose();

        public abstract Task OpenAsync();

        public abstract Task ReceiveAsync();

        public abstract Task SendAsync(byte[] message);

        public abstract Task AddMessageAsync(byte[] message);
    }
}
