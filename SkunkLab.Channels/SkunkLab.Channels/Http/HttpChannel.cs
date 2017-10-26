using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Http
{
    public abstract class HttpChannel : IChannel
    {
        public static HttpChannel Create(HttpRequestMessage request)
        {
            return new HttpServerChannel(request);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType)
        {
            return new HttpServerChannel(endpoint, resourceUriString, contentType);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            return new HttpServerChannel(endpoint, resourceUriString, contentType, securityToken);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate)
        {
            return new HttpServerChannel(endpoint, resourceUriString, contentType, certificate);
        }

        public static HttpChannel Create(Uri requestUri, string resourceUriString, string securityToken, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            return new HttpClientChannel(requestUri, securityToken, resourceUriString, contentType, observers, indexes, token);
        }

        public static HttpChannel Create(Uri requestUri, string resourceUriString, X509Certificate2 certificate, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            return new HttpClientChannel(requestUri, certificate, resourceUriString, contentType, observers, indexes, token);
        }

        //public static HttpChannel Create(Uri requestUri, string resourceUriString, string contentType = null, IEnumerable<Observer> observers = null, IEnumerable<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        //{
        //    return new HttpClientChannel(requestUri, resourceUriString, contentType, observers, indexes, token);
        //}

        public abstract int Port { get; internal set; }
        public abstract bool IsConnected { get;  }
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

        
        public abstract Task CloseAsync();

        public abstract void Dispose();

        public abstract Task OpenAsync();

        public abstract Task ReceiveAsync();

        public abstract Task SendAsync(byte[] message);

        public abstract Task AddMessageAsync(byte[] message);
    }
}
