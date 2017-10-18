using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace Piraeus.Adapters
{
    public class CoapRequestDispatcher : ICoapRequestDispatch
    {
        public CoapRequestDispatcher(CoapSession session, IChannel channel)
        {
            container = new Dictionary<string, Tuple<string, string>>();
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
            this.channel = channel;
            this.session = session;
        }

        private IChannel channel;
        private CoapSession session;
        private Dictionary<string, Tuple<string, string>> container;  //resource, subscription + leaseKey
        private Dictionary<string, IMessageObserver> ephemeralObservers; //subscription, observer
        private Dictionary<string, IMessageObserver> durableObservers;   //subscription, observer
        private System.Timers.Timer leaseTimer;

        /// <summary>
        /// Unsubscribes an ephemeral subscription from a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Delete(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());

            if(container.ContainsKey(uri.Resource))
            {
                Tuple<string, string> tuple = container[uri.Resource];
                Task task = GraphManager.RemoveSubscriptionObserverAsync(tuple.Item1, tuple.Item2);
                Task.WhenAll(task);   
                
                if(ephemeralObservers.ContainsKey(tuple.Item1))
                {
                    ephemeralObservers.Remove(tuple.Item1);
                }

                if (message.MessageType == CoapMessageType.NonConfirmable)
                {
                    return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Deleted, message.Token);
                }
                else
                {
                    return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Deleted, message.Token);
                }
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.NotFound);
            }
            

        }

        /// <summary>
        /// Subscribes to a resource, but is not Coap Observe
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Get(CoapMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CoAP observe, observes a resource for a subscription.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Observe(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());

            if(!container.ContainsKey(uri.Resource))
            {
                IResource resource = GraphManager.GetResource(uri.Resource);
                SubscriptionMetadata submetadata = new SubscriptionMetadata();
                submetadata.IsEphemeral = true;
                string subscriptionUriString = GraphManager.SubscribeAsync(submetadata, resource).GetAwaiter().GetResult();
                MessageObserver observer = new MessageObserver();
                observer.OnNotify += Observer_OnNotify;
                TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                string leaseKey = GraphManager.ObserveMessagesAsync(subscriptionUriString, leaseTime, observer).GetAwaiter().GetResult();
                ephemeralObservers.Add(subscriptionUriString, observer);

                if (!container.ContainsKey(uri.Resource)) //ensure resource is not already subscribed
                {
                    container.Add(uri.Resource, new Tuple<string, string>(subscriptionUriString, leaseKey));
                }

                
            }

            return null;

        }

        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            byte[] message = ProtocolTransition.ConvertToCoap(session, e.Message);
            Task task = channel.SendAsync(message);
            Task.WhenAll(task);
        }

        /// <summary>
        /// Publishing a message to a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Post(CoapMessage message)
        {
            Task<CoapMessage> task = PostAsync(message);
            Task.WhenAll<CoapMessage>(task);
            return task.Result;
        }

        private async Task<CoapMessage> PostAsync(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            IResource resource = await GraphManager.GetResourceAsync(uri.Resource);
            ResourceMetadata metadata = await resource.GetMetadataAsync();
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (await CanPublishAsync(metadata))
            {
                EventMessage msg = new EventMessage(message.ContentType.Value.ConvertToContentType(), uri.Resource, ProtocolType.COAP, message.Encode());

                if (uri.Indexes == null)
                {
                    await resource.PublishAsync(msg);
                }
                else
                {
                    await resource.PublishAsync(msg, new List<KeyValuePair<string, string>>(uri.Indexes));
                }

                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }
        }

        /// <summary>
        /// Subscribe message for ephemeral subscription
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Put(CoapMessage message)
        {
            Task<CoapMessage> task = PutAsync(message);
            Task.WhenAll<CoapMessage>(task);
            return task.Result;
        }

        private async Task<CoapMessage> PutAsync(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            IResource resource = await GraphManager.GetResourceAsync(uri.Resource);
            ResourceMetadata metadata = await resource.GetMetadataAsync();
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;
            
            if (await CanSubscribeAsync(metadata))
            {
                SubscriptionMetadata submetadata = new SubscriptionMetadata();
                submetadata.IsEphemeral = true;

                //subscribe to the resource
                string subscriptionUriString = await GraphManager.SubscribeAsync(submetadata, resource);

                //create and observer and wire up event to receive notifications
                MessageObserver observer = new MessageObserver();
                observer.OnNotify += Observer_OnNotify;

                //set the observer in the subscription with the lease lifetime
                TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                string leaseKey = await GraphManager.ObserveMessagesAsync(subscriptionUriString, leaseTime, observer);

                //add the lease key to the list of ephemeral observers
                ephemeralObservers.Add(subscriptionUriString, observer);

                //add the resource, subscription, and lease key the container
                if (!container.ContainsKey(uri.Resource))
                {
                    container.Add(uri.Resource, new Tuple<string, string>(subscriptionUriString, leaseKey));
                }

                //ensure the lease timer is running 50% faster than the lease.
                EnsureLeaseTimer();

                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CoapRequestDispatcher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Subscribe Private Method

        public async Task<bool> CanSubscribeAsync(ResourceMetadata metadata)
        {
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

            if (metadata.RequireEncryptedChannel && !channel.IsEncrypted)
            {
                await Log.LogWarningAsync("Subscribe resource {0} requires an encrypted channel.  Channel {1} is not encrypted.", metadata.ResourceUriString, channel.Id);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to subscribe/unsubscribe for resource {1}", session.Identity, metadata.ResourceUriString);
            }

            return authz;
        }

        private ResponseCodeType Subscribe(string uriString)
        {
            CoapUri uri = new CoapUri(uriString);
            ResourceMetadata metadata = GraphManager.GetResourceMetadata(uri.Resource);

            if (container.ContainsKey(uri.Resource))
            {
                return ResponseCodeType.Created;
            }

            if (metadata != null)
            {
                //get access control grain
                IAccessControl accessControl = GraphManager.GetAccessControl(metadata.SubscribePolicyUriString);
                if (accessControl != null)
                {
                    //make access control check
                    Task<bool> task = accessControl.IsAuthorizedAsync(Thread.CurrentPrincipal.Identity as ClaimsIdentity);

                    if (task.GetAwaiter().GetResult())
                    {
                        //subscribe to resource
                        IResource resource = GraphManager.GetResource(uri.Resource);
                        SubscriptionMetadata submetadata = new SubscriptionMetadata();
                        submetadata.IsEphemeral = true;
                        string subscriptionUriString = GraphManager.SubscribeAsync(submetadata, resource).GetAwaiter().GetResult();
                        MessageObserver observer = new MessageObserver();
                        observer.OnNotify += Observer_OnNotify;
                        TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                        string leaseKey = GraphManager.ObserveMessagesAsync(subscriptionUriString, leaseTime, observer).GetAwaiter().GetResult();
                        ephemeralObservers.Add(subscriptionUriString, observer);

                        container.Add(uri.Resource, new Tuple<string, string>(subscriptionUriString, leaseKey));

                        if (leaseTimer == null)  //make sure lease timer is running
                        {
                            leaseTimer = new System.Timers.Timer(10.0);
                            leaseTimer.Elapsed += LeaseTimer_Elapsed;
                            leaseTimer.Start();
                        }

                        return ResponseCodeType.Created;
                    }
                    else //not authorized
                    {
                        return ResponseCodeType.Unauthorized;
                    }
                }
                else
                {
                    //no access control policy cannot subscribe
                    return ResponseCodeType.Unauthorized;
                }
            }
            else
            {
                return ResponseCodeType.NotFound;
            }

        }
        #endregion

        #region Publish Utilities
        private async Task<bool> CanPublishAsync(ResourceMetadata metadata)
        {
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

            if (metadata.RequireEncryptedChannel && !channel.IsEncrypted)
            {
                await Log.LogWarningAsync("Publish resource {0} requires an encrypted channel.  Channel {1} is not encrypted.", metadata.ResourceUriString, channel.Id);
                return false;
            }

            IAccessControl accessControl = await GraphManager.GetAccessControlAsync(metadata.PublishPolicyUriString);

            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            bool authz = await accessControl.IsAuthorizedAsync(identity);

            if (!authz)
            {
                await Log.LogWarningAsync("Identity {0} is not authorized to publish to resource {1}", session.Identity, metadata.ResourceUriString);
            }

            return authz;
        }
        #endregion

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

        private void EnsureLeaseTimer()
        {
            if (leaseTimer == null)  //make sure lease timer is running
            {
                leaseTimer = new System.Timers.Timer(10.0);
                leaseTimer.Elapsed += LeaseTimer_Elapsed;
                leaseTimer.Start();
            }
        }
    }
}
