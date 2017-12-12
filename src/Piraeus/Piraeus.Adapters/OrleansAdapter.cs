using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Capl.Authorization;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Grains;
using SkunkLab.Diagnostics.Logging;

namespace Piraeus.Adapters
{
    public class OrleansAdapter : OrleansAdapterBase
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

        public override async Task<bool> CanPublishAsync(string resourceUriString, bool channelEncrypted)
        {
            ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(resourceUriString);

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

            AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(metadata.PublishPolicyUriString);

            if (policy == null)
            {
                await Log.LogWarningAsync("Publish policy URI {0} did not return an authorization policy.", metadata.PublishPolicyUriString);
                return false;
            }

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = policy.Evaluate(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity is not authorized to publish to resource {0}", metadata.ResourceUriString);
            }

            return authz;
        }

        public override async Task<bool> CanSubscribeAsync(string resourceUriString, bool channelEncrypted)
        {            
            ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(resourceUriString);

            if (metadata == null)
            {
                await Log.LogWarningAsync("Cannot subscribe. Resource metadata is null.");
                return false;
            }

            if (!metadata.Enabled)
            {
                await Log.LogWarningAsync("Cannot subscribe. Resource {0} is disabled.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow)
            {
                await Log.LogWarningAsync("Cannot subscribe. Resource {0} has expired.", metadata.ResourceUriString);
                return false;
            }

            if (metadata.RequireEncryptedChannel && !channelEncrypted)
            {
                await Log.LogWarningAsync("Cannot subscribe. Resource {0} requires an encrypted channel.", metadata.ResourceUriString);
                return false;
            }

            AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(metadata.SubscribePolicyUriString);
            
            if(policy == null)
            {
                await Log.LogWarningAsync("Subscription policy URI {0} did not return an authorization policy.", metadata.SubscribePolicyUriString);
                return false;
            }

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = policy.Evaluate(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity is not authorized to subscribe/unsubscribe for resource {1}", metadata.ResourceUriString);
            }

            return authz;
        }

        public override async Task<List<string>> LoadDurableSubscriptionsAsync(string identity)
        {
            List<string> list = new List<string>();

            IEnumerable<string> subscriptionUriStrings = await GraphManager.GetSubscriberSubscriptionsListAsync(identity);

            if(subscriptionUriStrings == null || subscriptionUriStrings.Count() == 0)
            {
                return null;
            }

            foreach (var item in subscriptionUriStrings)
            {
                if (!durableObservers.ContainsKey(item))
                {
                    MessageObserver observer = new MessageObserver();
                    observer.OnNotify += Observer_OnNotify;

                    //set the observer in the subscription with the lease lifetime
                    TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

                    string leaseKey = await GraphManager.AddSubscriptionObserverAsync(item, leaseTime, observer);

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

            if (subscriptionUriStrings.Count() > 0)
            {
                EnsureLeaseTimer();
            }

            return list.Count == 0 ? null : list;
        }

        public override async Task PublishAsync(EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            if(indexes == null || indexes.Count == 0)
            {
                await GraphManager.PublishAsync(message.ResourceUri, message);
            }
            else
            {
                await GraphManager.PublishAsync(message.ResourceUri, message, indexes);
            }
        }

        public override async Task<string> SubscribeAsync(string resourceUriString, SubscriptionMetadata metadata)
        {
            metadata.IsEphemeral = true;
            string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);

            //create and observer and wire up event to receive notifications
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += Observer_OnNotify;

            //set the observer in the subscription with the lease lifetime
            TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);

            string leaseKey = await GraphManager.AddSubscriptionObserverAsync(subscriptionUriString, leaseTime, observer);

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

        public override async Task UnsubscribeAsync(string resourceUriString)
        {
            //unsubscribe from resource
            if (container.ContainsKey(resourceUriString))
            {
                if (ephemeralObservers.ContainsKey(container[resourceUriString].Item1))
                {
                    await GraphManager.RemoveSubscriptionObserverAsync(container[resourceUriString].Item1, container[resourceUriString].Item2);
                    await GraphManager.UnsubscribeAsync(container[resourceUriString].Item1);
                    ephemeralObservers.Remove(container[resourceUriString].Item1);
                }

                container.Remove(resourceUriString);
            }
        }

        #region Dispose
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
        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            //signal the protocol adapter
            OnObserve?.Invoke(this, new ObserveMessageEventArgs(e.Message));
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

        private void LeaseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Dictionary<string, Tuple<string, string>>.Enumerator en = container.GetEnumerator();
            KeyValuePair<string, Tuple<string, string>>[] kvps = container.ToArray();

            Task leaseTask = Task.Factory.StartNew(async () =>
            {
                if (kvps != null && kvps.Length > 0)
                {
                    foreach (var kvp in kvps)
                    {
                        SubscriptionMetadata metadata = await GraphManager.GetSubscriptionMetadataAsync(kvp.Value.Item1);
                        //Task.WhenAll<SubscriptionMetadata>(task);
                        //SubscriptionMetadata metadata = task.Result;

                        if (metadata != null)
                        {
                            bool renewed = await GraphManager.RenewObserverLeaseAsync(kvp.Value.Item1, kvp.Value.Item2, TimeSpan.FromSeconds(60.0));
                            //Task.WhenAll<bool>(renewTask);
                            if (!renewed)
                            {
                                await Log.LogWarningAsync("Observer lease could not be renewed.");
                            }

                            //taskList.Add(subscription.RenewObserverLeaseAsync(en.Current.Value.Item2, TimeSpan.FromSeconds(20.0)));
                        }
                    }
                }
            });

            Task.WhenAll(leaseTask);

            




            //while (en.MoveNext())
            //{
            //    Task<SubscriptionMetadata> task = GraphManager.GetSubscriptionMetadataAsync(en.Current.Value.Item1);
            //    Task.WhenAll<SubscriptionMetadata>(task);
            //    SubscriptionMetadata metadata = task.Result;

            //    if (metadata != null)
            //    {
            //        Task<bool> renewTask = GraphManager.RenewObserverLeaseAsync(en.Current.Value.Item1, en.Current.Value.Item2, TimeSpan.FromSeconds(20.0));
            //        Task.WhenAll<bool>(renewTask);
            //        if(!renewTask.Result)
            //        {
            //            Log.LogWarningAsync("Observer lease could not be renewed.");
            //        }

            //        //taskList.Add(subscription.RenewObserverLeaseAsync(en.Current.Value.Item2, TimeSpan.FromSeconds(20.0)));
            //    }
            //    //ISubscription subscription = GraphManager.GetSubscription(en.Current.Value.Item1);
            //    //if (subscription.GetMetadataAsync().GetAwaiter().GetResult() != null)
            //    //{
            //    //    taskList.Add(subscription.RenewObserverLeaseAsync(en.Current.Value.Item2, TimeSpan.FromSeconds(20.0)));
            //    //}


            //}
        }

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
        #endregion
    }
}
