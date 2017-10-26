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

        public RestClient(Uri endpointUri, string resourceUriString, string contentType, string securityToken, List<KeyValuePair<string,string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri, resourceUriString, securityToken, contentType, null, indexes, token);
        }

        public RestClient(Uri endpointUri, string resourceUriString, string contentType, X509Certificate2 certificate, List<KeyValuePair<string, string>> indexes = null, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri, resourceUriString, certificate, contentType, null, indexes, token);
        }

        public RestClient(Uri endpointUri, string securityToken, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri, null, securityToken, null, observers, null, token);
        }

        public RestClient(Uri endpointUri, X509Certificate2 certificate, IEnumerable<Observer> observers = null, CancellationToken token = default(CancellationToken))
        {
            channel = ChannelFactory.Create(endpointUri, null, certificate, null, observers, null, token);
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
