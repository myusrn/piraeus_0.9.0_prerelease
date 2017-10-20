using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Diagnostics.Logging;

namespace Piraeus.Adapters
{
    public class OrleansAdapter : IOrleansAdapter
    {
        public OrleansAdapter()
        {
            container = new Dictionary<string, Tuple<string, string>>();
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
        }

        public event EventHandler<ObserveMessageEventArgs> OnObserve;   //signal protocol adapter

        private Dictionary<string, Tuple<string, string>> container;  //resource, subscription + leaseKey
        private Dictionary<string, IMessageObserver> ephemeralObservers; //subscription, observer
        private Dictionary<string, IMessageObserver> durableObservers;   //subscription, observer
        private System.Timers.Timer leaseTimer; //timer for leases
        private bool disposedValue = false; // To detect redundant calls

        #region Interface

        public async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            IResource resource = await GraphManager.GetResourceAsync(message.ResourceUri);
            await resource.PublishAsync(message, indexes);
        }
        public async Task<List<string>> LoadDurableSubscriptionsAsync(string identity)
        {
            List<string> list = new List<string>();
            IEnumerable<string> subscriptionUriStrings = await GraphManager.GetSubscriberSubscriptionsAsync(identity);
            foreach (var item in subscriptionUriStrings)
            {
                if (!durableObservers.ContainsKey(item))
                {
                    MessageObserver observer = new MessageObserver();
                    observer.OnNotify += Observer_OnNotify;

                    //set the observer in the subscription with the lease lifetime
                    TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                    string leaseKey = await GraphManager.ObserveMessagesAsync(item, leaseTime, observer);

                    //add the lease key to the list of ephemeral observers
                    durableObservers.Add(item, observer);
                    

                    //get the resource from the subscription
                    Uri uri = new Uri(item);
                    string resourceUriString = item.Replace(uri.Segments[uri.Segments.Length - 1], "");

                    list.Add(resourceUriString); //add to list to return

                    //add the resource, subscription, and lease key the container

                    if (!container.ContainsKey(resourceUriString))
                    {
                        container.Add(resourceUriString, new Tuple<string, string>(item, leaseKey));
                    }
                }
            }

            //ensure the lease timer is running 50% faster than the lease.
            if (subscriptionUriStrings.Count() > 0)
            {
                EnsureLeaseTimer();
            }

            return list.Count == 0 ? null : list;

        }

        public async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            metadata.IsEphemeral = true;
            IResource resource = await GraphManager.GetResourceAsync(resourceUriString);
            string subscriptionUriString = await GraphManager.SubscribeAsync(metadata, resource);

            //create and observer and wire up event to receive notifications
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += Observer_OnNotify;

            //set the observer in the subscription with the lease lifetime
            TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
            string leaseKey = await GraphManager.ObserveMessagesAsync(subscriptionUriString, leaseTime, observer);

            //add the lease key to the list of ephemeral observers
            ephemeralObservers.Add(subscriptionUriString, observer);

            //add the resource, subscription, and lease key the container
            if (!container.ContainsKey(resourceUriString))
            {
                container.Add(resourceUriString, new Tuple<string, string>(subscriptionUriString, leaseKey));
            }

            //ensure the lease timer is running
            EnsureLeaseTimer();

            return subscriptionUriString;
        }
        
        public async Task UnsubscribeAsync(string resourceUriString)
        {
            //unsubscribe from resource
            if (container.ContainsKey(resourceUriString))
            {
                if (ephemeralObservers.ContainsKey(container[resourceUriString].Item1))
                {
                    await GraphManager.RemoveSubscriptionObserverAsync(container[resourceUriString].Item1, container[resourceUriString].Item2);
                    ephemeralObservers.Remove(container[resourceUriString].Item1);
                }

                container.Remove(resourceUriString);
            }            
        }

        public async Task<bool> CanPublishAsync(string resourceUriString, bool channelEncrypted)
        {
            IResource resource = await GraphManager.GetResourceAsync(resourceUriString);
            ResourceMetadata metadata = await resource.GetMetadataAsync();

            if (metadata == null)
            {
                await Log.LogWarningAsync("Publish resource metadata is null.");
                return false;
            }

            if (!metadata.Enabled)
            {
                await Log.LogWarningAsync("Publish resource {0} is disabled.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                await Log.LogWarningAsync("Publish resource {0} has expired.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !channelEncrypted)
            {
                await Log.LogWarningAsync("Publish resource {0} requires an encrypted channel.", metadata.ResourceUriString);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to publish to resource {1}", identity.Name, metadata.ResourceUriString);
            }

            return authz;
        }

        public async Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted)
        {
            IResource resource = await GraphManager.GetResourceAsync(resourceUriString);
            ResourceMetadata metadata = await resource.GetMetadataAsync();

            if (metadata == null)
            {
                await Log.LogWarningAsync("Subscribe resource metadata is null.");
                return false;
            }

            if (!metadata.Enabled)
            {
                await Log.LogWarningAsync("Subscribe resource {0} is disabled.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                await Log.LogWarningAsync("Subscribe resource {0} has expired.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !channelEncrypted)
            {
                await Log.LogWarningAsync("Subscribe resource {0} requires an encrypted channel.", metadata.ResourceUriString);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to subscribe/unsubscribe for resource {1}", identity.Name, metadata.ResourceUriString);
            }

            return authz;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Task t1 = RemoveDurableObserversAsync();
                    Task.WhenAll(t1);

                    Task t2 = RemoveEphemeralObserversAsync();
                    Task.WhenAll(t2);

                    if (leaseTimer != null)
                    {
                        leaseTimer.Stop();
                        leaseTimer.Dispose();
                    }
                }

                container = null;
                ephemeralObservers = null;
                durableObservers = null;

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        
        #region private methods

        private async Task RemoveDurableObserversAsync()
        {
            List<string> list = new List<string>();

            if (durableObservers.Count > 0)
            {
                List<Task> taskList = new List<Task>();
                foreach (var item in durableObservers)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);
                    foreach (var lease in items)
                    {
                        list.Add(lease.Key);

                        if (durableObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task task = GraphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            taskList.Add(task);
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    await Task.WhenAll(taskList);
                }

                durableObservers.Clear();
                RemoveFromContainer(list);
            }
        }

        private async Task RemoveEphemeralObserversAsync()
        {
            List<string> list = new List<string>();
            if (ephemeralObservers.Count > 0)
            {
                List<Task> taskList = new List<Task>();
                foreach (var item in ephemeralObservers)
                {
                    IEnumerable<KeyValuePair<string, Tuple<string, string>>> items = container.Where((c) => c.Value.Item1 == item.Key);

                    foreach (var lease in items)
                    {
                        list.Add(lease.Key);
                        if (ephemeralObservers.ContainsKey(lease.Value.Item1))
                        {
                            Task task = GraphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2);
                            taskList.Add(task);
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    await Task.WhenAll(taskList);
                }

                ephemeralObservers.Clear();
                RemoveFromContainer(list);
            }
        }

        private void RemoveFromContainer(string subscriptionUriString)
        {
            List<string> list = new List<string>();
            var query = container.Where((c) => c.Value.Item1 == subscriptionUriString);

            foreach (var item in query)
            {
                list.Add(item.Key);
            }

            foreach (string item in list)
            {
                container.Remove(item);
            }
        }

        private void RemoveFromContainer(List<string> subscriptionUriStrings)
        {
            foreach (var item in subscriptionUriStrings)
            {
                RemoveFromContainer(item);
            }
        }
        
        private void EnsureLeaseTimer()
        {
            if (leaseTimer == null)
            {
                leaseTimer = new System.Timers.Timer(10.0);
                leaseTimer.Elapsed += LeaseTimer_Elapsed;
                leaseTimer.Start();
            }
        }

        #endregion

        #region events
        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            //signal the protocol adapter
            OnObserve?.Invoke(this, new ObserveMessageEventArgs(e.Message));
        }
        private void LeaseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<Task> taskList = new List<Task>();
            Dictionary<string, Tuple<string, string>>.Enumerator en = container.GetEnumerator();
            while (en.MoveNext())
            {
                ISubscription subscription = GrainClient.GrainFactory.GetGrain<ISubscription>(en.Current.Value.Item1);
                if (subscription.GetMetadataAsync().GetAwaiter().GetResult() != null)
                {
                    taskList.Add(subscription.RenewObserverLeaseAsync(en.Current.Value.Item2, TimeSpan.FromSeconds(20.0)));
                }
            }

            if (taskList.Count > 0)
            {
                Task.WhenAll(taskList);
            }
        }

        #endregion



    }
}
