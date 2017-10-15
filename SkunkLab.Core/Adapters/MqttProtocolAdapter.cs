using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.ServiceModel;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;

namespace SkunkLab.Core.Adapters
{
    public class MqttProtocolAdapter : ProtocolAdapter
    {
        public MqttProtocolAdapter(MqttConfig config, IChannel channel)
        {
            session = new MqttSession(config);
            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnSent += Channel_OnSent;
            Channel.OnRetry += Channel_OnRetry;
            Channel.OnStateChange += Channel_OnStateChange;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;
            ephemeralObservers = new Dictionary<string, IMessageObserver>();
            durableObservers = new Dictionary<string, IMessageObserver>();
            container = new Dictionary<string, Tuple<string, string>>();
        }

        public override event ProtocolAdapterCloseHandler OnClose;
        public override event ProtocolAdapterErrorHandler OnError;
        public override IChannel Channel { get; set; }
        private MqttSession session;
        private Dictionary<string, Tuple<string, string>> container;  //resource, subscription + leaseKey
        private Dictionary<string, IMessageObserver> ephemeralObservers; //subscription, observer
        private Dictionary<string, IMessageObserver> durableObservers;   //subscription, observer
        private System.Timers.Timer leaseTimer;
        private bool disposed;

        public override void Init()
        {
            session.OnPublish += Session_OnPublish;
            session.OnSubscribe += Session_OnSubscribe;
            session.OnUnsubscribe += Session_OnUnsubscribe;
            session.OnDisconnect += Session_OnDisconnect;
            session.OnSubscribeWithReturn += Session_OnSubscribeWithReturn;

        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {
                disposed = true;
                session.Dispose();                
                RemoveDurableObservers();
                RemoveEphemeralObservers();

                if (leaseTimer != null)
                {
                    leaseTimer.Stop();
                    leaseTimer.Dispose();
                }

                durableObservers = null;
                ephemeralObservers = null;
                container = null;
            }
        }


        #region Session Events
        private void Session_OnSubscribe(object sender, MqttMessageEventArgs args)
        {
            //ephemeral subscribe to resource
            SubscribeMessage message = (SubscribeMessage)args.Message;
            foreach (var item in message.Topics)
            {
                MqttUri mqttUri = new MqttUri(item.Key);
                ResourceMetadata metadata = GraphManager.GetResourceMetadata(mqttUri.Resource);
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
                            IResource resource = GraphManager.GetResource(mqttUri.Resource);
                            SubscriptionMetadata submetadata = new SubscriptionMetadata();
                            string subscriptionUriString = GraphManager.SubscribeAsync(submetadata, resource).GetAwaiter().GetResult();
                            MessageObserver observer = new MessageObserver();
                            observer.OnNotify += Observer_OnNotify;
                            TimeSpan leaseTime = TimeSpan.FromSeconds(20.0);
                            string leaseKey = GraphManager.ObserveMessagesAsync(subscriptionUriString, leaseTime, observer).GetAwaiter().GetResult();

                            if (!container.ContainsKey(mqttUri.Resource)) //ensure resource is not already subscribed
                            {
                                container.Add(mqttUri.Resource, new Tuple<string, string>(subscriptionUriString, leaseKey));
                            }

                            if (leaseTimer == null)  //make sure lease timer is running
                            {
                                leaseTimer = new System.Timers.Timer(10.0);
                                leaseTimer.Elapsed += LeaseTimer_Elapsed;
                                leaseTimer.Start();
                            }
                        }
                        else //not authorized
                        {
                        }
                    }
                    else
                    {
                        //no access control policy cannot publish
                    }
                }
            }
        }
        
        private List<string> Session_OnSubscribeWithReturn(object sender, MqttMessageEventArgs args)
        {
            List<string> list = new List<string>();
            SubscribeMessage message = (SubscribeMessage)args.Message;
            foreach (var item in message.Topics)
            {
                MqttUri mqttUri = new MqttUri(item.Key);
                ResourceMetadata metadata = GraphManager.GetResourceMetadata(mqttUri.Resource);
                if (metadata != null)
                {
                    //get access control grain
                    IAccessControl accessControl = GraphManager.GetAccessControl(metadata.SubscribePolicyUriString);
                    if (accessControl != null)
                    {
                        if(accessControl.IsAuthorizedAsync(Thread.CurrentPrincipal.Identity as ClaimsIdentity).GetAwaiter().GetResult())
                        {
                            list.Add(item.Key);
                        }
                    }
                }
            }

            return list;
        }

        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            //clean up observers and remove ephemeral subscriptions
            //close the channel
            Task task = Channel.CloseAsync();
            Task.WhenAll(task);
        }

        private void Session_OnUnsubscribe(object sender, MqttMessageEventArgs args)
        {
            UnsubscribeMessage message = (UnsubscribeMessage)args.Message;
            List<string> list = new List<string>();
            //unsubscribe from resource
            foreach(var item in message.Topics)
            {                
                MqttUri uri = new MqttUri(item.ToLower(CultureInfo.InvariantCulture));
                if(container.ContainsKey(uri.Resource))
                {
                    if(ephemeralObservers.ContainsKey(container[uri.Resource].Item1))
                    {
                        Task unsubTask = GraphManager.UnsubscribeAsync(container[uri.Resource].Item1);
                        Task.WhenAll(unsubTask);
                        ephemeralObservers.Remove(container[uri.Resource].Item1);
                        list.Add(uri.Resource);
                    }
                }
            }

            foreach(var item in list)
            {
                container.Remove(item);
            }
        }
        

        private void Session_OnPublish(object sender, MqttMessageEventArgs args)
        {
            //publish to resource
            PublishMessage message = (PublishMessage)args.Message;
            MqttUri mqttUri = new MqttUri(message.Topic);

            ResourceMetadata metadata = GraphManager.GetResourceMetadata(mqttUri.Resource);
            if(metadata != null)
            {
                //get access control grain
                IAccessControl accessControl = GraphManager.GetAccessControl(metadata.PublishPolicyUriString);
                if(accessControl != null)
                {
                    //make access control check
                    Task<bool> task = accessControl.IsAuthorizedAsync(Thread.CurrentPrincipal.Identity as ClaimsIdentity);
                    task.GetAwaiter().GetResult();
                    if (task.Result)
                    {
                        //publish to resource
                        IResource resource = GraphManager.GetResource(mqttUri.Resource);
                        Task pubTask = resource.PublishAsync(new Piraeus.Core.Messaging.EventMessage(mqttUri.ContentType, mqttUri.Resource, Piraeus.Core.Messaging.ProtocolType.MQTT, args.Message.Encode()));
                        Task.WhenAll(pubTask);
                    }
                    else //not authorized
                    {
                    }
                }
                else
                {
                    //no access control policy cannot publish
                }
            }
        }

        #endregion

        #region Observer events
        private void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            //convert message to protocol and send on channel
            


        }

        #endregion
        
        #region Timer events

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

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;            
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            Task task = null;
            try
            {
                MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
                MqttMessageHandler handler = MqttMessageHandler.Create(session, msg);
                Task<MqttMessage> responseTask = handler.ProcessAsync();
                Task<MqttMessage>.WhenAll(responseTask);
                MqttMessage response = responseTask.Result;

                if (response != null)
                {
                    Task sendTask = Channel.SendAsync(response.Encode());
                    Task.WhenAll(sendTask);
                }
            }
            catch(AggregateException ae)
            {
                task = Channel.CloseAsync();
            }
            catch(Exception ex)
            {
                task = Channel.CloseAsync();
            }

            if(task != null)
            {
                Task.WhenAll(task);
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnRetry(object sender, ChannelRetryEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnSent(object sender, ChannelSentEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            RemoveEphemeralObservers();
            RemoveDurableObservers();
            
            if(leaseTimer != null)
            {
                leaseTimer.Stop();
            }

            container.Clear();
        }

        #endregion

        #region Utilities

        private void RemoveDurableObservers()
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
                            taskList.Add(GraphManager.RemoveSubscriptionObserverAsync(item.Key, lease.Value.Item2));
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    Task.WhenAll(taskList);
                }

                durableObservers.Clear();
                RemoveFromContainer(list);
            }
        }

        private void RemoveEphemeralObservers()
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
                            taskList.Add(GraphManager.RemoveSubscriptionObserverAsync(lease.Value.Item1, lease.Value.Item2));                                            
                        }
                    }
                }

                if (taskList.Count > 0)
                {
                    Task.WhenAll(taskList);
                }

                ephemeralObservers.Clear();
                RemoveFromContainer(list);
            }
        }

        private void RemoveFromContainer(string subscriptionUriString)
        {
            List<string> list = new List<string>();
            var query = container.Where((c) => c.Value.Item1 == subscriptionUriString);

            foreach(var item in query)
            {
                list.Add(item.Key);
            }

            foreach(string item in list)
            {
                container.Remove(item);
            }
        }

        private void RemoveFromContainer(List<string> subscriptionUriStrings)
        {
            foreach(var item in subscriptionUriStrings)
            {
                RemoveFromContainer(item);
            }
        }
        #endregion





    }
    
}
