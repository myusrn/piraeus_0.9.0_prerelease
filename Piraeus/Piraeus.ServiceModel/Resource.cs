using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    [StorageProvider(ProviderName = "store")]
    public class Resource : Grain<ResourceState>, IResource
    {

        public override Task OnActivateAsync()
        {           
            if(State.ErrorLeases == null)
            {
                State.ErrorLeases = new Dictionary<string, IErrorObserver>();
            }

            if(State.LeaseExpiry == null)
            {
                State.LeaseExpiry = new Dictionary<string, Tuple<DateTime, string>>();
            }

            if(State.MetricLeases == null)
            {
                State.MetricLeases = new Dictionary<string, IMetricObserver>();
            }

            if(State.Subscriptions == null)
            {
                State.Subscriptions = new Dictionary<string, ISubscription>();
            }

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            Task task = WriteStateAsync();
            Task.WhenAll(task);

            return Task.CompletedTask;
        }

        public async Task ClearAsync()
        {
            foreach(ISubscription item in State.Subscriptions.Values)
            {
                string id = await item.GetIdAsync();
                if (id != null)
                {
                    await UnsubscribeAsync(id);
                }
            }

            if (State.Metadata != null)
            {
                IResourceList resourceList = GrainFactory.GetGrain<IResourceList>("resourcelist");
                if (!await resourceList.Contains(State.Metadata.ResourceUriString))
                {
                    await resourceList.RemoveAsync(State.Metadata.ResourceUriString);
                }
            }

            State.Subscriptions.Clear();
            await ClearStateAsync();

            
        }

        public async Task PublishAsync(EventMessage message)
        {
            if(message == null)
            {
                return;
            }

            State.MessageCount++;
            State.ByteCount += message.Message.LongLength;
            State.LastMessageTimestamp = DateTime.UtcNow;

            List<Task> taskList = new List<Task>();

            foreach(var item in State.Subscriptions.Values)
            {
                taskList.Add(item.NotifyAsync(message));
            }

            await Task.WhenAll(taskList);

            await NotifyMetricsAsync();
        }

        public async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            if (message == null)
            {
                return;
            }

            if (indexes == null || indexes.Count == 0)
            {
                await PublishAsync(message);
            }
            else
            {
                State.MessageCount++;
                State.ByteCount += message.Message.LongLength;
                State.LastMessageTimestamp = DateTime.UtcNow;

                List<Task> taskList = new List<Task>();

                foreach (var item in State.Subscriptions.Values)
                {
                    taskList.Add(item.NotifyAsync(message, indexes));
                }

                await Task.WhenAll(taskList);

                await NotifyMetricsAsync();
            }
        }

        public async Task SubscribeAsync(ISubscription subscription)
        {
            string id = await subscription.GetIdAsync();

            if(State.Subscriptions.ContainsKey(id))
            {
                State.Subscriptions.Remove(id);
            }

            State.Subscriptions.Add(id, subscription);

            SubscriptionMetadata metadata = await subscription.GetMetadataAsync();
            if(metadata != null && !string.IsNullOrEmpty(metadata.Identity) 
                                && string.IsNullOrEmpty(metadata.NotifyAddress)
                                && !metadata.IsEphemeral)
            {
                ISubscriber subscriber = GrainFactory.GetGrain<ISubscriber>(metadata.Identity);
                await subscriber.AddSubscriptionAsync(metadata.SubscriptionUriString);
            }

        }

        public async Task UnsubscribeAsync(string subscriptionUriString)
        {
            if (State.Subscriptions.ContainsKey(subscriptionUriString))
            {
                ISubscription subscription = State.Subscriptions[subscriptionUriString];
                SubscriptionMetadata metadata = await subscription.GetMetadataAsync();

                if (metadata != null && !string.IsNullOrEmpty(metadata.Identity)
                                    && string.IsNullOrEmpty(metadata.NotifyAddress)
                                    && !metadata.IsEphemeral)
                {
                    ISubscriber subscriber = GrainFactory.GetGrain<ISubscriber>(metadata.Identity);
                    await subscriber.RemoveSubscriptionAsync(metadata.SubscriptionUriString);
                }

                await subscription.ClearAsync();
            }

            State.Subscriptions.Remove(subscriptionUriString);
        }

        public async Task<ResourceMetadata> GetMetadataAsync()
        {
            if(!IsDisabled())
            {
                return await Task.FromResult<ResourceMetadata>(State.Metadata);
            }
            else
            {
                return await Task.FromResult<ResourceMetadata>(null);
            }
        }

        public async Task UpsertMetadataAsync(ResourceMetadata metadata)
        {
            if(metadata == null)
            {
                return;
            }

            if(metadata.ResourceUriString != this.GetPrimaryKeyString())
            {
                //resource does not match the grain identity
                Exception ex = new GrainIdentityMismatchException(String.Format("Resource metadata {0} does not match grain identity {1}", State.Metadata.ResourceUriString, this.GetPrimaryKeyString()));                
                await NotifyErrorAsync(ex);
                throw ex;                  
            }

            State.Metadata = metadata;

            IResourceList resourceList = GrainFactory.GetGrain<IResourceList>("resourcelist");
            if(!await resourceList.Contains(metadata.ResourceUriString))
            {
                await resourceList.AddAsync(metadata.ResourceUriString);
            }
        }

        public async Task<IEnumerable<string>> GetSubscriptionListAsync()
        {
            return await Task.FromResult<IEnumerable<string>>(State.Subscriptions.Keys.ToArray());
        }

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer)
        {
            string leaseKey = Guid.NewGuid().ToString();
            State.MetricLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Metric"));
            return await Task.FromResult<string>(leaseKey);
        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer)
        {
            string leaseKey = Guid.NewGuid().ToString();
            State.ErrorLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Error"));
            return await Task.FromResult<string>(leaseKey);
        }

        public async Task RemoveObserverAsync(string leaseKey)
        {
            var metricQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Error");

            State.LeaseExpiry.Remove(leaseKey);

            if (metricQuery.Count() == 1)
            {
                State.MetricLeases.Remove(leaseKey);
            }

            if (errorQuery.Count() == 1)
            {
                State.ErrorLeases.Remove(leaseKey);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime)
        {
            if (State.LeaseExpiry.ContainsKey(leaseKey))
            {
                Tuple<DateTime, string> value = State.LeaseExpiry[leaseKey];
                Tuple<DateTime, string> newValue = new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), value.Item2);
                State.LeaseExpiry[leaseKey] = newValue;
                return await Task.FromResult<bool>(true);
            }

            return await Task.FromResult<bool>(false);
        }

        private async Task NotifyMetricsAsync()
        {
            if (!IsDisabled() && State.ErrorLeases.Count > 0)
            {
                //try
                //{

                    Task task = Task.Factory.StartNew(() =>
                    {
                        foreach (var item in State.MetricLeases.Values)
                        {
                            item.NotifyMetrics(new CommunicationMetrics(State.Metadata.ResourceUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp.Value, State.LastErrorTimestamp));
                        }
                    });

                    await Task.WhenAll(task);
                //}
                //catch (AggregateException ae)
                //{
                //    ae.Flatten().InnerException;
                //}
                //catch (Exception ex)
                //{
                //}
            }
        }

        private async Task NotifyErrorAsync(Exception ex)
        {
            if (!IsDisabled() && State.ErrorLeases.Count > 0)
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    foreach (var item in State.ErrorLeases.Values)
                    {
                        item.NotifyError(State.Metadata.ResourceUriString, ex);
                    }
                });

                await Task.WhenAll(task);
            }
        }

        private bool IsDisabled()
        {
            return State.Metadata == null;
        }
    }
}
