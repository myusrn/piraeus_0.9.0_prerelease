using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;

namespace Piraeus.Grains
{
    public interface ISubscription : IGrainWithStringKey
    {
        Task<string> GetIdAsync();
        Task UpsertMetadataAsync(SubscriptionMetadata metadata);

        Task<SubscriptionMetadata> GetMetadataAsync();
        Task<string> AddObserverAsync(TimeSpan lifetime, IMessageObserver observer);
        Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer);
        Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer);
        Task RemoveObserverAsync(string leaseKey);
        Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime);
        Task NotifyAsync(EventMessage message);
        Task NotifyAsync(EventMessage message, List<KeyValuePair<string, string>> indexes);

        Task ClearAsync();
    }
}
