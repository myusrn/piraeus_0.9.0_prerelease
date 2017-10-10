using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Piraeus.Actors
{
    public interface IResource : IGrainWithStringKey
    {
        Task UpsertMetadataAsync(ResourceMetadata metadata);

        Task<ResourceMetadata> GetMetadataAsync();

        Task ClearAsync();

        Task<List<string>> GetSubscriptionsAsync();

        Task<ISubscription> GetSubscription(string subscriptionUri);
        

        

        


    }
}
