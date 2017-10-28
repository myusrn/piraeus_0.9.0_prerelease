using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using Piraeus.Grains;

namespace TestSiloClient
{
    class Program
    {
        private static string subscribedSubscriptionUriString;
        static void Main(string[] args)
        {
            Console.WriteLine("Orleans Client Test...");
            Console.ReadKey();

            Console.WriteLine("Initializing Orleans Client");
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Olreans client initialized");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            Task task = RunTests();
            Task.WaitAll(task);


            Console.WriteLine("Done");
            Console.ReadKey();


        }

        static void Init()
        {
            var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
            Orleans.GrainClient.Initialize(config);
        }

        static async Task RunTests()
        {
            
            await UpsertResourceMetdataResourceTestAsync();
            await GetMetadataResourceTestAsync();
            await UpsertSubscriptionMetadataTestAsync();
            await GraphManagerGetSubscriptionMetadataAsync();
            await SubscribeToResourceTestAsync();
            await UnsubscribeFromResourceTestAsync();
            await CheckResourceMetricObserverAsync();
            await CheckResourceErrorObserverAsync();
            await GraphManagerAddSubscriberAsync();
            await GraphManagerListSubscriberSubscriptionsAsync();
            await GraphManagerRemoveSubscriberAsync();
            //await GraphManagerClearSubscriberSubscriptionsAsync();
           
            await MessageObserverLeaseExpiresAsync();
            await MessageObserverRenewandExpireLeaseAsync();
        }



        #region Graph Manager Tests

        #region Resource Metadata Tests
        static async Task UpsertResourceMetdataResourceTestAsync()
        {            
            string resourceUriString = "http://www.example.org/resource1";

            try
            {
                ResourceMetadata metadata = GetResourceMetadataLocal(resourceUriString);

                await GraphManager.UpsertResourceMetadataAsync(metadata);
                ResourceMetadata rm = await GraphManager.GetResourceMetadata(metadata.ResourceUriString);
                bool result = metadata.ResourceUriString == rm.ResourceUriString;

                WriteResult("Upsert Resource Metadata", result);
            }
            catch (Exception ex)
            {
                WriteResult("Upsert Resource Metadata", false, ex);
            }
        }
        static async Task GetMetadataResourceTestAsync()
        {
            string resourceUriString = "http://www.example.org/resource1";

            try
            {
                ResourceMetadata metadata = await GraphManager.GetResourceMetadata(resourceUriString);
                bool result = resourceUriString == metadata.ResourceUriString;
                WriteResult("Get Resource Metadata", result);
            }
            catch (Exception ex)
            {
                WriteResult("Get Resource Metadata", false, ex);
            }


        }

        static async Task SubscribeToResourceTestAsync()
        {
            string resourceUriString = "http://www.example.org/resource1";
            string identity = "myidentity";

            try
            {
                SubscriptionMetadata metadata = GetSubscriptionMetadataLocal(identity, true);
                subscribedSubscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);
                WriteResult("Subscribe To Resource", true);
            }
            catch (Exception ex)
            {
                WriteResult("Subscribe To Resource", false, ex);
            }
        }

        static async Task UnsubscribeFromResourceTestAsync()
        {
            try
            {
                await GraphManager.UnsubscribeAsync(subscribedSubscriptionUriString);
                WriteResult("Unsubscribe From Resource", true);
            }
            catch (Exception ex)
            {
                WriteResult("Unsubscribe From Resource", false, ex);
            }
        }

        static async Task CheckResourceMetricObserverAsync()
        {
            long count = 0;
            string resourceUriString = "http://www.example.org/resource1";
            MetricObserver observer = new MetricObserver();            
            observer.OnNotify += (o, m) => { count = m.Metrics.MessageCount; };
            string leaseKey = await GraphManager.AddResourceMetricObserver(resourceUriString, observer, TimeSpan.FromSeconds(10.0));

            //send a message
            EventMessage message = new EventMessage("text/plain", resourceUriString, ProtocolType.REST, Encoding.UTF8.GetBytes("hello"));
            await GraphManager.PublishAsync(resourceUriString, message);

            Thread.Sleep(1000);
            WriteResult("Test Resource Metric Observer", count == 1);

        }

        static async Task CheckResourceErrorObserverAsync()
        {
            string resourceUriString = "http://www.example.org/resource1";
            ErrorObserver observer = new ErrorObserver();
            Exception error = null;
            observer.OnNotify += (o, m) => { error = m.Error; };
            string leaseKey = await GraphManager.AddResourceErrorObserver(resourceUriString, observer, TimeSpan.FromSeconds(10.0));

            //send a message that generates an error
            //EventMessage message = new EventMessage("text/plain", resourceUriString, ProtocolType.REST, Encoding.UTF8.GetBytes("hello"));
            await GraphManager.PublishAsync(resourceUriString, null);

            Thread.Sleep(1000);

            WriteResult("Check Resource Error Observer", error != null && error.Message == "Value cannot be null.\r\nParameter name: resource publish message");

        }

        #endregion

        #region Subscription Metadata Tests

        private static async Task UpsertSubscriptionMetadataTestAsync()
        {
            string identity = "myidentity";
            SubscriptionMetadata metadata = GetSubscriptionMetadataLocal(identity, true);
            metadata.SubscriptionUriString = "http://www.example.org/resource1/subscription1";
            await GraphManager.UpsertSubscriptionMetadataAsync(metadata);
            SubscriptionMetadata sm = await GraphManager.GetSubscriptionMetadataAsync(metadata.SubscriptionUriString);
            WriteResult("Upsert Subscription Metadata", sm.SubscriptionUriString == metadata.SubscriptionUriString);
        }

        public static async Task GraphManagerGetSubscriptionMetadataAsync()
        {
            SubscriptionMetadata metadata = await GraphManager.GetSubscriptionMetadataAsync("http://www.example.org/resource1/subscription1");
            WriteResult("Get Subscription Metadata", metadata.SubscriptionUriString != null);
            //Console.WriteLine("Get Subscription Metadata {0}", !string.IsNullOrEmpty(metadata.SubscriptionUriString));
        }

        #endregion

        #region Subscriber Tests

        public static async Task GraphManagerAddSubscriberAsync()
        {
            string identity = "myidentity";
            string subscriptionUriString = "http://www.example.org/resource1/sub1";
            await GraphManager.AddSubscriberSubscriptionAsync(identity, subscriptionUriString);
            IEnumerable<string> subscriptionList = await GraphManager.ListSubscriberSubscriptionsAsync(identity);
            List<string> list = new List<string>(subscriptionList);

            WriteResult("Add Subscriber Subscription", list.Count == 1);
        }

        public static async Task GraphManagerListSubscriberSubscriptionsAsync()
        {
            string identity = "myidentity";
            IEnumerable<string> items = await GraphManager.ListSubscriberSubscriptionsAsync(identity);
            List<string> list = new List<string>(items);
            WriteResult("List Subscriber Subscriptions", list.Count == 1);
        }

        public static async Task GraphManagerRemoveSubscriberAsync()
        {
            string identity = "myidentity";
            string subscriptionUriString = "http://www.example.org/resource1/sub1";
            await GraphManager.AddSubscriberSubscriptionAsync(identity, subscriptionUriString);
            IEnumerable<string> subs = await GraphManager.ListSubscriberSubscriptionsAsync(identity);
            List<string> list1 = new List<string>(subs);

            await GraphManager.RemoveSubscriberSubscriptionAsync(identity, subscriptionUriString);
            IEnumerable<string> subscriptionList = await GraphManager.ListSubscriberSubscriptionsAsync(identity);
            List<string> list2 = new List<string>(subscriptionList);
            WriteResult("Remove Subscriber Subscription", list1.Count == 1 && list2.Count == 0);
        }



        public static async Task GraphManagerClearSubscriberSubscriptionsAsync()
        {
            string identity = "myidentity";
            await GraphManager.ClearSubscriberSubscriptionsAsync(identity);
            Console.WriteLine("Graph Manager cleared subscriber subscriptions");
        }

        public static async Task MessageObserverLeaseExpiresAsync()
        {
            //create the resource
            string resourceUriString = "http://www.example.org/resource1";

            ResourceMetadata rm = GetResourceMetadataLocal(resourceUriString);
            await GraphManager.UpsertResourceMetadataAsync(rm);

            IEnumerable<string> items = await GraphManager.GetResourceSubscriptionListAsync(resourceUriString);
            foreach(var item in items)
            {
                await GraphManager.UnsubscribeAsync(item);
            }

            //subscribe to the resource
            SubscriptionMetadata sm = GetSubscriptionMetadataLocal("myidentity", true);
            string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, sm);

            //create an observer
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += (o, m) => { Console.WriteLine("Message {0}", m.Timestamp); };

            //get lease to observer the resource
            string leaseKey = await GraphManager.AddSubscriptionMessageObserverAsync(subscriptionUriString, TimeSpan.FromSeconds(5.0), observer);

            //check the number of subscriptions; should be = 1
            List<string> list1 = new List<string>(await GraphManager.GetResourceSubscriptionListAsync(resourceUriString));

            //send a message
            EventMessage message = new EventMessage("text/plain", resourceUriString, ProtocolType.REST, Encoding.UTF8.GetBytes("hello"));
            await GraphManager.PublishAsync(resourceUriString, message);

            Thread.Sleep(12000); //expire the lease

            //check the number of subscriptions;  should be 0
            List<string> list2 = new List<string>(await GraphManager.GetResourceSubscriptionListAsync(resourceUriString));

            WriteResult("Let Lease Expire", list1.Count == 1 && list2.Count == 0);

        }

        public static async Task MessageObserverRenewandExpireLeaseAsync()
        {
            //create the resource
            string resourceUriString = "http://www.example.org/resource1";
            ResourceMetadata rm = GetResourceMetadataLocal(resourceUriString);
            await GraphManager.UpsertResourceMetadataAsync(rm);

            //subscribe to the resource
            SubscriptionMetadata sm = GetSubscriptionMetadataLocal("myidentity", true);
            string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, sm);

            //create an observer
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += (o, m) => { Console.WriteLine("Message {0}", m.Timestamp); };

            //get lease to observer the resource
            string leaseKey = await GraphManager.AddSubscriptionMessageObserverAsync(subscriptionUriString, TimeSpan.FromSeconds(5.0), observer);

            //check the number of subscriptions; should be = 1
            List<string> list1 = new List<string>(await GraphManager.GetResourceSubscriptionListAsync(resourceUriString));

            //send a message
            EventMessage message = new EventMessage("text/plain", resourceUriString, ProtocolType.REST, Encoding.UTF8.GetBytes("hello"));
            await GraphManager.PublishAsync(resourceUriString, message);

            await GraphManager.RenewObserverLeaseAsync(subscriptionUriString, leaseKey, TimeSpan.FromSeconds(5.0));

            //check the number of subscriptions;  should be 0
            List<string> list2 = new List<string>(await GraphManager.GetResourceSubscriptionListAsync(resourceUriString));

            Thread.Sleep(14000); //expire the lease

            //check the number of subscriptions;  should be 0
            List<string> list3 = new List<string>(await GraphManager.GetResourceSubscriptionListAsync(resourceUriString));

            WriteResult("Renew Lease Then Let Expire", list1.Count == 1 && list2.Count == 1 && list3.Count == 0);

        }

        #endregion
        #endregion



        #region Grain Tests

        static async Task AddResourceMetadataAsync()
        {
            try
            {
                ResourceMetadata metadata = new ResourceMetadata()
                {
                    RequireEncryptedChannel = false,
                    Enabled = true,
                    PublishPolicyUriString = "http://www.example.org/pubpolicy",
                    ResourceUriString = "http://www.example.org/resource1",
                    SubscribePolicyUriString = "http://www.example.org/subpolicy"
                };

                IResource resource = GrainClient.GrainFactory.GetGrain<IResource>(metadata.ResourceUriString);
                await resource.UpsertMetadataAsync(metadata);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("AE ADD {0}", ae.Flatten().InnerException.Message);

            }
            catch (Exception ex)
            {
                Console.WriteLine("EX ADD {0}", ex.Message);

            }
        }



        static async Task GetResourceMetadataAsync()
        {
            try
            {
                IResource resource = GrainClient.GrainFactory.GetGrain<IResource>("http://www.example.org/resource1");
                ResourceMetadata metadata = await resource.GetMetadataAsync();

                if (metadata == null)
                {
                    Console.WriteLine("Metadata is null");
                }
                else
                {
                    Console.WriteLine("Metadata Id = {0}", metadata.ResourceUriString);
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("AE GET {0}", ae.Flatten().InnerException.Message);

            }
            catch (Exception ex)
            {
                Console.WriteLine("EX GET {0}", ex.Message);

            }
        }
        #endregion


        #region Utilities

        private static ResourceMetadata GetResourceMetadataLocal(string resourceUriString)
        {
            return new ResourceMetadata()
            {
                RequireEncryptedChannel = false,
                Enabled = true,
                PublishPolicyUriString = "http://www.example.org/pubpolicy",
                ResourceUriString = resourceUriString,
                SubscribePolicyUriString = "http://www.example.org/subpolicy"
            };
        }

        private static SubscriptionMetadata GetSubscriptionMetadataLocal(string identity, bool ephemeral)
        {
            return new SubscriptionMetadata()
            {
                Identity = identity,
                IsEphemeral = ephemeral
            };
        }

        private static void WriteResult(string testName, bool result, Exception error = null)
        {
            Console.ForegroundColor = result ? ConsoleColor.White : ConsoleColor.Yellow;
            Console.WriteLine("{0} {1}", testName.PadRight(40), result);
            if (error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error.Message);
                Console.WriteLine("------------------------------------------------------------------------");
                Console.WriteLine();
            }
        }

        #endregion

    }
}
