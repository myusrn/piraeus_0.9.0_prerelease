using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;

namespace Piraeus.Grains
{
    public interface IResource : IGrainWithStringKey
    {

        Task SubscribeAsync(ISubscription subscription);

        Task UnsubscribeAsync(string subscriptionUriString);

        Task UpsertMetadataAsync(ResourceMetadata metadata);

        Task<ResourceMetadata> GetMetadataAsync();

        Task<IEnumerable<string>> GetSubscriptionListAsync();
        Task PublishAsync(EventMessage message);

        Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes);

        Task ClearAsync();

        Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer);
        Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer);

        Task RemoveObserverAsync(string leaseKey);

        Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime);



    }
}
