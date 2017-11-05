using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Channels;

namespace Piraeus.Clients.Rest
{
    public class RestClient
    {       

        /// <summary>
        /// REST client to send messages
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="securityToken"></param>
        /// <param name="indexes"></param>
        /// <param name="token"></param>
        public RestClient(Uri endpointUri, string resourceUriString, string contentType, string securityToken, List<KeyValuePair<string,string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri.ToString(), resourceUriString, contentType, securityToken, indexes);
            //channel = ChannelFactory.Create(endpointUri, resourceUriString, securityToken, contentType, null, indexes, token);
        }

        /// <summary>
        /// REST client to send messages
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="certificate"></param>
        /// <param name="indexes"></param>
        /// <param name="token"></param>
        public RestClient(Uri endpointUri, string resourceUriString, string contentType, X509Certificate2 certificate, List<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri.ToString(), resourceUriString, contentType, certificate, indexes);
            //channel = ChannelFactory.Create(endpointUri, resourceUriString, certificate, contentType, null, indexes);
        }

        /// <summary>
        /// REST client to receive messages
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="securityToken"></param>
        /// <param name="observers"></param>
        /// <param name="token"></param>
        public RestClient(Uri endpointUri, string securityToken, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri.ToString(), securityToken, observers, token);
            //channel = ChannelFactory.Create(endpointUri, null, securityToken, "application/json", observers, null, token);
        }

        /// <summary>
        /// REST client to receive messages
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="certificate"></param>
        /// <param name="observers"></param>
        /// <param name="token"></param>
        public RestClient(Uri endpointUri, X509Certificate2 certificate, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri.ToString(), certificate, observers, token);
            //channel = ChannelFactory.Create(endpointUri, null, certificate, null, observers, null, token);
        }
        private IChannel channel;

        public async Task SendAsync(byte[] message)
        {
            if (!channel.IsConnected)
            {
                await channel.OpenAsync();
            }

            await channel.SendAsync(message);
        }

        public async Task ReceiveAsync()
        {
            if (!channel.IsConnected)
            {
                await channel.OpenAsync();
            }

            await channel.ReceiveAsync();
        }
    }
}
