using System;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Core.Utilities;
using System.Collections.Generic;
using Piraeus.Core.Messaging;
using Capl.Authorization;

namespace Piraeus.Grains
{
    public class GraphManager
    {
        #region Resource Operations
        public static IResource GetResource(string resourceUriString)
        {
            Uri uri = new Uri(resourceUriString);
            string uriString = uri.ToCanonicalString(false);
            return GrainClient.GrainFactory.GetGrain<IResource>(uriString);
        }

        public static async Task UpsertResourceMetadataAsync(ResourceMetadata metadata)
        {
            Uri uri = new Uri(metadata.ResourceUriString);
            metadata.ResourceUriString = uri.ToCanonicalString(false);
            IResource resource = GetResource(metadata.ResourceUriString);
            await resource.UpsertMetadataAsync(metadata);
        }

        public static async Task<ResourceMetadata> GetResourceMetadata(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.GetMetadataAsync();
        }
               
        public static async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            Uri uri = new Uri(resourceUriString);
            string subscriptionUriString = uri.ToCanonicalString(true) + Guid.NewGuid().ToString();
            metadata.SubscriptionUriString = subscriptionUriString;


            //Add the metadata to the subscription
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);

            //Add the subscription to the resource
            IResource resource = GetResource(uri.ToCanonicalString(false));
            await resource.SubscribeAsync(subscription);
            return subscriptionUriString;
        }

        public static async Task UnsubscribeAsync(string subscriptionUriString)
        {
            //get the resource to unsubscribe
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = uri.ToCanonicalString(false, true);
            IResource resource = GetResource(resourceUriString);

            //unsubscribe from the resource
            await resource.UnsubscribeAsync(subscriptionUriString);
        }

        public static async Task PublishAsync(string resourceUriString, EventMessage message)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.PublishAsync(message);
        }

        public static async Task PublishAsync(string resourceUriString, EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.PublishAsync(message, indexes);
        }

        public static async Task<IEnumerable<string>> GetResourceSubscriptionListAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.GetSubscriptionListAsync();
        }
        
        public static async Task<string> AddResourceMetricObserver(string resourceUriString, MetricObserver observer, TimeSpan lifetime)
        {
            IMetricObserver metricObserver = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(observer);
            IResource resource = GetResource(resourceUriString);
            return await resource.AddObserverAsync(lifetime, metricObserver);
        }

        public static async Task<string> AddResourceErrorObserver(string resourceUriString, ErrorObserver observer, TimeSpan lifetime)
        {
            IErrorObserver errorObserver = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(observer);
            IResource resource = GetResource(resourceUriString);
            return await resource.AddObserverAsync(lifetime, errorObserver);
        }

        public static async Task<bool> RenewResoureObserverLease(string resourceUriString, string leaseKey, TimeSpan lifetime)
        {
            IResource resource = GetResource(resourceUriString);
            return await resource.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        public static async Task ClearResourceAsync(string resourceUriString)
        {
            IResource resource = GetResource(resourceUriString);
            await resource.ClearAsync();
        }
        
        #endregion

        #region Subscription Operations

        public static ISubscription GetSubscription(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            return GrainClient.GrainFactory.GetGrain<ISubscription>(uri.ToCanonicalString(false));
        }

        public static async Task UpsertSubscriptionMetadataAsync(SubscriptionMetadata metadata)
        {
            ISubscription subscription = GetSubscription(metadata.SubscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);
        }

        public static async Task<SubscriptionMetadata> GetSubscriptionMetadataAsync(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            ISubscription subscription = GetSubscription(uri.ToCanonicalString(false));
            return await subscription.GetMetadataAsync();
        }

        public static async Task<string> AddSubscriptionMessageObserverAsync(string subscriptionUriString, TimeSpan lifetime, MessageObserver observer)
        {
            IMessageObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IMessageObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        public static async Task<string> AddObserverAsync(string subscriptionUriString, TimeSpan lifetime, IMetricObserver observer)
        {
            IMetricObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        public static async Task<string> AddObserverAsync(string subscriptionUriString, TimeSpan lifetime, IErrorObserver observer)
        {
            IErrorObserver observerRef = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(observer);
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.AddObserverAsync(lifetime, observerRef);
        }

        public static async Task RemoveObserverAsync(string subscriptionUriString, string leaseKey)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.RemoveObserverAsync(leaseKey);
        }

        public static async Task<bool> RenewObserverLeaseAsync(string subscriptionUriString, string leaseKey, TimeSpan lifetime)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            return await subscription.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        public static async Task SubscriptionClearAsync(string subscriptionUriString)
        {
            ISubscription subscription = GetSubscription(subscriptionUriString);
            await subscription.ClearAsync();
        }

        #endregion

        #region Subscriber Operations

        public static ISubscriber GetSubscriber(string identity)
        {            
            return GrainClient.GrainFactory.GetGrain<ISubscriber>(identity.ToLowerInvariant());
        }

        public static async Task AddSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);
            await subscriber.AddSubscriptionAsync(subscriptionUriString);
        }

        public static async Task RemoveSubscriberSubscriptionAsync(string identity, string subscriptionUriString)
        {
            ISubscriber subscriber = GetSubscriber(identity);
            await subscriber.RemoveSubscriptionAsync(subscriptionUriString);
        }

        public static async Task<IEnumerable<string>> ListSubscriberSubscriptionsAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);
            return await subscriber.GetSubscriptionsAsync();
        }

        public static async Task ClearSubscriberSubscriptionsAsync(string identity)
        {
            ISubscriber subscriber = GetSubscriber(identity);
            await subscriber.ClearAsync();
        }

        #endregion

        #region ResourceList
        
        public async Task<IEnumerable<string>> GetResourceListAsync()
        {
            IResourceList resourceList = GrainClient.GrainFactory.GetGrain<IResourceList>("resourcelist");
            return await resourceList.GetListAsync();
        }

        #endregion

        #region Access Control

        public IAccessControl GetAccessControlPolicy(string policyUriString)
        {
            Uri uri = new Uri(policyUriString);
            string uriString = uri.ToCanonicalString(false);
            return GrainClient.GrainFactory.GetGrain<IAccessControl>(uriString);
        }

        public async Task UpsertAcessControlPolicyAsync(string policyUriString, AuthorizationPolicy policy)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.UpsertPolicyAsync(policy);
        }

        public async Task ClearAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            await accessControl.ClearAsync();
        }

        public async Task<AuthorizationPolicy> GetAccessControlPolicyAsync(string policyUriString)
        {
            IAccessControl accessControl = GetAccessControlPolicy(policyUriString);
            return await accessControl.GetPolicyAsync();
        }

        #endregion


    }
}
