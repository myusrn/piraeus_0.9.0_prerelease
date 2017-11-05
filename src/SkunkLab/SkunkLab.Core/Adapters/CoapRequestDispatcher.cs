using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;

namespace SkunkLab.Core.Adapters
{
    public class CoapRequestDispatcher : ICoapRequestDispatch
    {
        public CoapRequestDispatcher(string identity, IChannel channel)
        {
            this.identity = identity;
            container = new Dictionary<string, Tuple<string, string>>();
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
            this.channel = channel;
            LoadDurableSubscriptions();
        }

        private string identity;
        private IChannel channel;
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
        /// Not implemented as GET is reserved for Observe.
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

                if (leaseTimer == null)  //make sure lease timer is running
                {
                    leaseTimer = new System.Timers.Timer(10.0);
                    leaseTimer.Elapsed += LeaseTimer_Elapsed;
                    leaseTimer.Start();
                }
            }

            return null;

        }

        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
           
        }

        /// <summary>
        /// Publishing a message to a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Post(CoapMessage message)
        {
            ResponseCodeType rct;
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            IResource resource = GraphManager.GetResource(uri.Resource);
            ResourceMetadata metadata = resource.GetMetadataAsync().GetAwaiter().GetResult();
            if(metadata == null)
            {
                IAccessControl accessControl = GraphManager.GetAccessControl(metadata.PublishPolicyUriString);
                if(accessControl.IsAuthorizedAsync(Thread.CurrentPrincipal.Identity as ClaimsIdentity).GetAwaiter().GetResult())
                {
                    Task task = null;
                    EventMessage eventMessage = new EventMessage(message.ContentType.Value.ConvertToContentType(), metadata.ResourceUriString, Piraeus.Core.Messaging.ProtocolType.COAP, message.Encode());
                    //new EventMessage(ProtocolType.Coap, metadata.ResourceUriString, message.ContentType.Value.ConvertToContentType(), metadata.Audit, message.Payload);
                    if(uri.Indexes != null)
                    {
                        task = resource.PublishAsync(eventMessage);
                    }
                    else
                    {
                        task = resource.PublishAsync(eventMessage, new List<KeyValuePair<string,string>>(uri.Indexes));
                    }

                    Task.WhenAll(task);                   
                }
                else
                {
                    rct = ResponseCodeType.Unauthorized;
                }


                
            }
            else
            {
                
            }
        }


        /// <summary>
        /// Subscribe to a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <remarks>Using this method requires that POST is still as a publish to subscriber.</remarks>
        public CoapMessage Put(CoapMessage message)
        {
            try
            {
                ResponseCodeType rct = Subscribe(message.ResourceUri.ToString());

                if (message.MessageType == CoapMessageType.NonConfirmable)
                {
                    return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, rct, message.Token);
                }
                else
                {
                    return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, rct, message.Token);
                }
            }
            catch(AggregateException ae)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.InternalServerError, message.Token);
            }
            catch(Exception ex)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.InternalServerError, message.Token);
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

        #region Load Durable Subscriptions
        private void LoadDurableSubscriptions()
        {
            IEnumerable<string> subscriptions = GraphManager.GetSubscriberSubscriptionsAsync(identity).GetAwaiter().GetResult();
            foreach(var item in subscriptions)
            {
                MessageObserver observer = new MessageObserver();
                observer.OnNotify += Observer_OnNotify;
                TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                string leaseKey = GraphManager.ObserveMessagesAsync(item, leaseTime, observer).GetAwaiter().GetResult();
                durableObservers.Add(item, observer);

                Uri uri = new Uri(item);
                string resourceUriString = item.Replace(uri.Segments[uri.Segments.Length - 1], "");
                container.Add(resourceUriString, new Tuple<string, string>(item, leaseKey));
            }
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
    }
}
