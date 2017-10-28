using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Piraeus.GrainInterfaces;

namespace Piraeus.Grains
{
    public class Subscriber : Grain<SubscriberState>, ISubscriber
    {
        #region Activate/Deactivate
        public override Task OnActivateAsync()
        {
            if(State.Container == null)
            {
                State.Container = new List<string>();
            }

            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }

        #endregion

        #region List/Add/Remove Subscriptions
        public async Task<IEnumerable<string>>  GetSubscriptionsAsync()
        {
            return await Task.FromResult<IEnumerable<string>>(State.Container);
        }

        public async Task AddSubscriptionAsync(string subscriptionUriString)
        {
            if(!State.Container.Contains(subscriptionUriString))
            {
                State.Container.Add(subscriptionUriString);
            }

            await Task.CompletedTask;
        }

        public async Task RemoveSubscriptionAsync(string subscriptionUriString)
        {
            State.Container.Remove(subscriptionUriString);
            await Task.CompletedTask;
        }

        #endregion

        #region Clear
        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }

        #endregion
    }
}
