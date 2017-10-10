using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Grains;
using Orleans;
using Piraeus.Core.Metadata;
using Capl.Authorization;

namespace Piraeus.ServiceModel
{
    public class GraphManager
    {
        #region Resources
        public static async Task<IResource> GetResourceAsync(string resourceUriString)
        {
            return await Task.FromResult<IResource>(GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString));
        }

        public static async Task AddResourceAsync(ResourceMetadata metadata)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(metadata.ResourceUriString);
            await resource.UpsertMetadataAsync(metadata);
        }

        public static async Task RemoveResourceAsync(string resourceUriString)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);
            await resource.ClearAsync();
        }

        public static async Task<ResourceMetadata> GetResourceMetadataAsync(string resourceUriString)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);
            return await resource.GetMetadataAsync();
        }

        public static async Task UpdateResourceMetadata(ResourceMetadata metadata)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(metadata.ResourceUriString);
            await resource.UpsertMetadataAsync(metadata);
        }

        #endregion

        #region Subscriptions

        public static async Task<SubscriptionMetadata> GetSubscriptionMetadataAsync(string subscriptionUriString)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);
            return await subscription.GetMetadataAsync();
        }

        public static async Task UpsertMetadata(SubscriptionMetadata metadata)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(metadata.SubscriptionUriString);
            await subscription.UpsertMetadataAsync(metadata);
        }

        public static async Task<string> SubscribeAsync(SubscriptionMetadata metadata, IResource resource)
        {
            string id = Guid.NewGuid().ToString();
            ResourceMetadata resourceMetadata = await resource.GetMetadataAsync();

            if (resourceMetadata == null)
            {
                return null;
            }

            string subscriptionUriString = resourceMetadata.ResourceUriString.LastIndexOf('/') == resourceMetadata.ResourceUriString.Length - 1 ? resourceMetadata.ResourceUriString + id : resourceMetadata.ResourceUriString + "/" + id;
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);
            metadata.SubscriptionUriString = subscriptionUriString;
            await subscription.UpsertMetadataAsync(metadata);
            await resource.SubscribeAsync(subscription);

            return await Task.FromResult<string>(subscriptionUriString);
        }

        public static async Task UnsubscribeAsync(string subscriptionUriString, IResource resource)
        {
            await resource.UnsubscribeAsync(subscriptionUriString);
           
        }

        public static async Task UnsubscribeAsync(string subscriptionUriString)
        {
            Uri uri = new Uri(subscriptionUriString);
            string resourceUriString = subscriptionUriString.Replace(uri.Segments[uri.Segments.Length - 1], "");
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);
            await UnsubscribeAsync(subscriptionUriString, resource);

        }


        #endregion

        #region Observers

        public static async Task<string> ObserveMessagesAsync(string subscriptionUriString, TimeSpan lifetime, MessageObserver msgObserver)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);

            IMessageObserver observer = await GrainClient.GrainFactory.CreateObjectReference<IMessageObserver>(msgObserver);
            
            return await subscription.AddObserverAsync(lifetime, observer);
        }

        public static async Task<string> ObserveSubscriptionErrorsAsync(string subscriptionUriString, TimeSpan lifetime, ErrorObserver errorObserver)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);

            IErrorObserver observer = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(errorObserver);

            return await subscription.AddObserverAsync(lifetime, observer);
        }

        public static async Task<string> ObserverSubscriptionMetricsAsync(string subscriptionUriString, TimeSpan lifetime, MetricObserver metricObsever)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);

            IMetricObserver observer = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(metricObsever);

            return await subscription.AddObserverAsync(lifetime, observer);
        }

        public static async Task<string> ObserveResourceErrorsAsync(string resourceUriString, TimeSpan lifetime, ErrorObserver errorObserver)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);

            IErrorObserver observer = await GrainClient.GrainFactory.CreateObjectReference<IErrorObserver>(errorObserver);

            return await resource.AddObserverAsync(lifetime, observer);
        }

        public static async Task<string> ObserveResourceMetricsAsync(string resourceUriString, TimeSpan lifetime, MetricObserver metricObsever)
        {
            IResource subscription = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);

            IMetricObserver observer = await GrainClient.GrainFactory.CreateObjectReference<IMetricObserver>(metricObsever);

            return await subscription.AddObserverAsync(lifetime, observer);
        }

        public static async Task RemoveSubscriptionObserverAsync(string subscriptionUriString, string leaseKey)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);
            await subscription.RemoveObserverAsync(leaseKey);
        }

        public static async Task RemoveResourceObserverAsync(string resourceUriString, string leaseKey)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);
            await resource.RemoveObserverAsync(leaseKey);
        }


        public static async Task RenewSubscriptionLeaseAsync(string subscriptionUriString, string leaseKey, TimeSpan lifetime)
        {
            ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(subscriptionUriString);
            await subscription.RenewObserverLeaseAsync(leaseKey, lifetime);
        }

        public static async Task RenewResourceLeaseAsync(string resourceUriString, string leaseKey, TimeSpan lifetime)
        {
            IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(resourceUriString);
            await resource.RenewObserverLeaseAsync(leaseKey, lifetime);
        }


        #endregion

        #region Access Control

        public async Task UpsertPolicy(AuthorizationPolicy policy)
        {
            IAccessControl accessControl = GrainClient.GrainFactory.GetGrain<IAccessControl>(policy.PolicyId.ToString());
            await accessControl.UpsertPolicyAsync(policy);
        }

        public async Task RemovePolicy(string policyId)
        {
            IAccessControl accessControl = GrainClient.GrainFactory.GetGrain<IAccessControl>(policyId);
            await accessControl.ClearAsync();
        }

        #endregion
    }
}
