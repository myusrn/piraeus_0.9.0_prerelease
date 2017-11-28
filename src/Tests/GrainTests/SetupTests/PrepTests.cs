using System;
using System.Threading.Tasks;
using Capl.Authorization;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;

namespace GrainTests.SetupTests
{
    public class PrepTests
    {
        public async Task AddAccessControlPolicy(AuthorizationPolicy policy)
        {
            await GraphManager.UpsertAcessControlPolicyAsync(policy.PolicyId.ToString(), policy);
        }

        public async Task<AuthorizationPolicy> GetAccessControlPolicy(string policyId)
        {
            return await GraphManager.GetAccessControlPolicyAsync(policyId);
        }

        public async Task AddResourceMetadata(ResourceMetadata metadata)
        {
            await GraphManager.UpsertResourceMetadataAsync(metadata);
        }

        public async Task<ResourceMetadata> GetResourceMetadata(string resourceUriString)
        {
            return await GraphManager.GetResourceMetadataAsync(resourceUriString);
        }

        public async Task<string> Subscribe(string resourceUriString, SubscriptionMetadata metadata)
        {
            return await GraphManager.SubscribeAsync(resourceUriString, metadata);
        }

        public async Task ObserveSubscription(string subscriptionUriString, TimeSpan lifetime, MessageObserver observer)
        {
            await GraphManager.AddSubscriptionObserverAsync(subscriptionUriString, lifetime, observer);
        }

        public async Task Publish(string resourceUriString, EventMessage message)
        {
            await GraphManager.PublishAsync(resourceUriString, message);
        }

        public async Task Unsubscribe(string subscriptionUriString)
        {
            await GraphManager.UnsubscribeAsync(subscriptionUriString);
        }
    }
}
