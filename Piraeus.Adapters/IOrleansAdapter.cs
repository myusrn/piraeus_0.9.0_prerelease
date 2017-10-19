using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;

namespace Piraeus.Adapters
{
    public interface IOrleansAdapter : IDisposable
    {
        Task<List<string>> LoadDurableSubscriptionsAsync(string identity);

        Task PublishAsync(EventMessage message, List<KeyValuePair<string,string>> indexes = null);

        Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata);

        Task UnsubscribeAsync(string resourceUriString);

        Task<bool> CanPublishAsync(string resourceUriString, bool channelEncrypted);

        Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted);


        
    }
}
