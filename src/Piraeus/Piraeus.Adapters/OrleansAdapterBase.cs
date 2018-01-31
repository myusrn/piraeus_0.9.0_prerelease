using System.Collections.Generic;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;

namespace Piraeus.Adapters
{
    public abstract class OrleansAdapterBase
    {
        public abstract Task<List<string>> LoadDurableSubscriptionsAsync(string identity);

        public abstract Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes = null);

        public abstract Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata);

        public abstract Task UnsubscribeAsync(string resourceUriString);

        public abstract Task<bool> CanPublishAsync(ResourceMetadata metadata, bool channelEncrypted);

        public abstract Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted);
    }
}
