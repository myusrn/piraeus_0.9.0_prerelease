using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    [StorageProvider(ProviderName = "store")]
    public class Subscriber : Grain<SubscriberState>, ISubscriber
    {
        public override Task OnActivateAsync()
        {
            if(State.Container == null)
            {
                State.Container = new List<string>();
            }

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            Task task = WriteStateAsync();
            Task.WhenAll(task);
            return Task.CompletedTask;
        }
        public async Task AddSubscriptionAsync(string subscriptionUriString)
        {
            if(!State.Container.Contains(subscriptionUriString))
            {
                State.Container.Add(subscriptionUriString);
            }

            await Task.CompletedTask;
        }

        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }

        public async Task<IEnumerable<string>> GetSubscriptionsAsync()
        {
            return await Task.FromResult<IEnumerable<string>>(State.Container.ToArray());
        }

        public async Task RemoveSubscriptionAsync(string subscriptionUriString)
        {
            State.Container.Remove(subscriptionUriString);
            await Task.CompletedTask;
        }
    }
}
