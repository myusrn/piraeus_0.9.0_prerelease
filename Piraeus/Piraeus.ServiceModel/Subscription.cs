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
    public class Subscription : Grain<SubscriptionState>, ISubscription
    {
        public Subscription()
        {            
        }

        [NonSerialized]
        private Queue<EventMessage> memoryMessageQueue;

        [NonSerialized]
        IDisposable leaseTimer;

        [NonSerialized]
        IDisposable messageQueueTimer;

        public override Task OnActivateAsync()
        {
            SetupMessageStorageQueues();

            if(State.LeaseExpiry == null)
            {
                State.LeaseExpiry = new Dictionary<string, Tuple<DateTime, string>>();
            }

            if(State.MessageLeases == null)
            {
                State.MessageLeases = new Dictionary<string, IMessageObserver>();
            }

            if(State.MessageLeases == null)
            {
                State.MessageLeases = new Dictionary<string, IMessageObserver>();
            }

            if(State.ErrorLeases == null)
            {
                State.ErrorLeases = new Dictionary<string, IErrorObserver>();
            }
                        
            if(State.MessageLeases.Count > 0 || State.MetricLeases.Count > 0 || State.ErrorLeases.Count > 0)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiry, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            CheckQueue(null);
            
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {   
            Task task = WriteStateAsync();
            Task.WhenAll(task);

            return Task.CompletedTask;
        }

        public async Task<string> GetIdAsync()
        {
            return await Task.FromResult<string>(State.Metadata == null ? null : State.Metadata.SubscriptionUriString);
        }

        public async Task<SubscriptionMetadata> GetMetadataAsync()
        {
            return await Task.FromResult<SubscriptionMetadata>(State.Metadata);
        }

        public async Task UpsertMetadataAsync(SubscriptionMetadata metadata)
        {       
            if(metadata == null)
            {
                return;
            }

            State.Metadata = metadata;
            SetupMessageStorageQueues();            

            await Task.CompletedTask;
        }

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMessageObserver observer)
        {
            if(IsDisabled())
            {
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MessageLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Message"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiry, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);
        }

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer)
        {
            if (IsDisabled())
            {
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MetricLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Metric"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiry, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);
        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer)
        {
            if(IsDisabled())
            {
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.ErrorLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Error"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiry, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);
        }

        public async Task RemoveObserverAsync(string leaseKey)
        {
            if (IsDisabled())
            {
                await Task.CompletedTask;
                return;
            }

            var messageQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Message");
            var metricQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Error");

            State.LeaseExpiry.Remove(leaseKey);

            if (messageQuery.Count() == 1)
            {
                State.MessageLeases.Remove(leaseKey);

                if (State.MessageLeases.Count == 0 && State.Metadata.IsEphemeral)
                {
                    await UnsubscribeFromResourceAsync();
                }
            }

            if (metricQuery.Count() == 1)
            {
                State.MetricLeases.Remove(leaseKey);
            }

            if(errorQuery.Count() == 1)
            {
                State.ErrorLeases.Remove(leaseKey);
            }

            await Task.CompletedTask;
        }
        public async Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime)
        {
            if (IsDisabled())
            {
                return await Task.FromResult<bool>(false);
            }

            if (State.LeaseExpiry.ContainsKey(leaseKey))
            {
                Tuple<DateTime, string> value = State.LeaseExpiry[leaseKey];
                Tuple<DateTime, string> newValue = new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), value.Item2);
                State.LeaseExpiry[leaseKey] = newValue;
                return await Task.FromResult<bool>(true);
            }

            return await Task.FromResult<bool>(false);
        }

        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }

        public async Task NotifyAsync(EventMessage message)
        {
            if(IsDisabled())
            {
                return;
            }

            if (message == null)
            {
                await NotifyErrorAsync(new ArgumentNullException("Message is null."));
                State.LastErrorTimestamp = DateTime.UtcNow;
            }
            
            State.MessageCount++;
            State.ByteCount += message.Message.LongLength;
            State.LastMessageTimestamp = DateTime.UtcNow;

            if(!string.IsNullOrEmpty(State.Metadata.NotifyAddress))
            {
                //send to passively connected subsystem
            }
            else if(State.MessageLeases.Count > 0)
            {
                //send to actively connected subsystem
                foreach (var observer in State.MessageLeases.Values)
                {
                    observer.Notify(message);
                }
            }
            else if(State.Metadata.DurableMessaging && State.Metadata.TTL.HasValue) //durable message queue
            {
                await QueueDurableMessageAsync(message);
            }
            else //in-memory message queue
            {
                await QueueInMemoryMessageAsync(message);
            }

            await NotifyMetricsAsync();         
        }

        public async Task NotifyAsync(EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            if (IsDisabled())
            {
                return;
            }

            if (indexes == null)
            {
                await NotifyAsync(message);
            }
            else
            {
                var query = indexes.Where((c) => State.Metadata.Indexes.Contains(new KeyValuePair<string, string>(c.Key, c.Value)));

                if (indexes.Count == query.Count())
                {
                    await NotifyAsync(message);
                }
                else
                {
                    State.MessageCount++;
                    State.ByteCount += message.Message.LongLength;
                    State.LastMessageTimestamp = DateTime.UtcNow;
                }
            }
        }

        private async Task NotifyMetricsAsync()
        {
            
            Task task = Task.Factory.StartNew(() =>
            {
                foreach (var item in State.MetricLeases.Values)
                {
                    item.NotifyMetrics(new CommunicationMetrics(State.Metadata.SubscriptionUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp.Value, State.LastErrorTimestamp));
                }
            });

            await Task.WhenAll(task);
        }

        private async Task NotifyErrorAsync(Exception ex)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                foreach (var item in State.ErrorLeases.Values)
                {
                    item.NotifyError(State.Metadata.SubscriptionUriString, ex);
                }
            });

            await Task.WhenAll(task);
        }

        private async Task QueueDurableMessageAsync(EventMessage message)
        {
            State.MessageQueue.Enqueue(message);

            if (messageQueueTimer == null)
            {
                messageQueueTimer = RegisterTimer(CheckQueue, null, TimeSpan.FromSeconds(1.0), State.Metadata.TTL.Value);
            }

            await Task.CompletedTask;
        }

        private async Task QueueInMemoryMessageAsync(EventMessage message)
        {
            memoryMessageQueue.Enqueue(message);

            if (messageQueueTimer == null)
            {
                messageQueueTimer = RegisterTimer(CheckQueue, null, TimeSpan.FromSeconds(1.0), State.Metadata.TTL.Value);
            }

            DelayDeactivation(State.Metadata.TTL.Value);

            await Task.CompletedTask;
        }

        private void SetupMessageStorageQueues()
        {
            if(State.Metadata == null || !State.Metadata.TTL.HasValue)
            {
                return;
            }

            if(State.Metadata.DurableMessaging)
            {
                if(State.MessageQueue == null)
                {
                    State.MessageQueue = new Queue<EventMessage>();
                }
            }
            else
            {
                if(memoryMessageQueue == null)
                {
                    memoryMessageQueue = new Queue<EventMessage>();
                }
            }
        }

        /// <summary>
        /// Dequeues messages from memory when an observer exists.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Task CheckQueue(object args)
        {
            if(IsDisabled())
            {
                return Task.CompletedTask;
            }

            if(State.MessageLeases.Count > 0)
            {
                Task task = null;
                if(memoryMessageQueue != null)
                {
                    task = DequeueAsync(memoryMessageQueue);

                    if(memoryMessageQueue.Count > 0)
                    {
                        DelayDeactivation(State.Metadata.TTL.Value);
                    }
                }

                if(State.MessageQueue != null)
                {
                    task = DequeueAsync(State.MessageQueue);
                }

                if(task != null)
                {
                    return Task.WhenAll(task);
                } 
            }

            return Task.CompletedTask;
        }

        private async Task UnsubscribeFromResourceAsync()
        {
            //unsubscribe from resource
            string uriString = State.Metadata.SubscriptionUriString;
            Uri uri = new Uri(uriString);
            string resourceUriString = uriString.Replace(uri.Segments[uri.Segments.Length - 1], "");
            IResource resource = GrainFactory.GetGrain<IResource>(resourceUriString);
            await resource.UnsubscribeAsync(State.Metadata.SubscriptionUriString);
            
            await ClearStateAsync();
        }

        private async Task DequeueAsync(Queue<EventMessage> queue)
        {
            while (queue.Count > 0)
            {
                EventMessage msg = queue.Dequeue();
                if (msg.Timestamp.Add(State.Metadata.TTL.Value) < DateTime.UtcNow)
                {
                    await NotifyAsync(msg);

                    if (State.Metadata.SpoolRate.HasValue)
                    {
                        await Task.Delay(State.Metadata.SpoolRate.Value);
                    }
                }
            }

            if (messageQueueTimer != null)
            {
                messageQueueTimer.Dispose();
            }
        }

        private Task CheckLeaseExpiry(object args)
        {
            var messageQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Message");
            var metricQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Error");

            List<string> messageLeaseKeyList = new List<string>(messageQuery.Select((c) => c.Key));
            List<string> metricLeaseKeyList = new List<string>(metricQuery.Select((c) => c.Key));
            List<string> errorLeaseKeyList = new List<string>(errorQuery.Select((c) => c.Key));
           
            foreach(var item in messageLeaseKeyList)
            {
                State.MessageLeases.Remove(item);
                State.LeaseExpiry.Remove(item);

                if (State.MessageLeases.Count == 0 && State.Metadata.IsEphemeral)
                {
                    Task task = UnsubscribeFromResourceAsync();
                    Task.WhenAll(task);
                }
            }

            foreach (var item in metricLeaseKeyList)
            {
                State.MetricLeases.Remove(item);
                State.LeaseExpiry.Remove(item);
            }

            foreach (var item in errorLeaseKeyList)
            {
                State.ErrorLeases.Remove(item);
                State.LeaseExpiry.Remove(item);
            }

            if(State.LeaseExpiry.Count == 0)
            {
                leaseTimer.Dispose();
            }

            return Task.CompletedTask;
        }

        private bool IsDisabled()
        {
            return State.Metadata == null;
        }

    }
}
