using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GrainTests.SetupTests;
using Capl.Authorization;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using System.Threading;
using System.IdentityModel;
using Piraeus.Core.Messaging;

namespace GrainTests
{
    class Program
    {
        private static string issuer = "http://www.skunklab.io/";
        private static string audience = "http://www.skunklab.io/";
        private static string key = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static List<Claim> claims;
        private static string tokenString;
        private static string resourceUriString = "http://www.skunklab.io/resource1";
        /*
         * Uri address = new Uri("http://www.skunklab.io/");
            string issuer = "http://www.skunklab.io/";
            string key = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            
         */

        static void Main(string[] args)
        {
            Console.WriteLine("----- GraphManager Tests -----");
            Console.ReadKey();

            int index = 0;
            while(!Orleans.GrainClient.IsInitialized)
            {
                var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                Orleans.GrainClient.Initialize(config);
                Thread.Sleep(2000);
                index++;

                if(index > 5)
                {
                    Console.WriteLine("Orleans client not initialized");
                    Console.ReadKey();
                    return;
                }
            }
            

            claims = new List<Claim>()
            {
                new Claim("http://www.skunklab.io/piraeus/role", "sub"),
                new Claim("http://www.example.org/name", "matts")
            };

            Identity identity = new Identity();
            tokenString = identity.GetIdentity(key, issuer, audience, claims);
            SkunkLab.Security.Authentication.SecurityTokenValidator.Validate(tokenString, SkunkLab.Security.Tokens.SecurityTokenType.JWT, key, issuer, audience);

            PrepTests tests = new PrepTests();
            AuthorizationPolicy pubPolicy = CreateAuthZPolicy("pub", "http://www.skunklab.io/resource1/pub");
            Task t1 = AddAccessControlPolicy(tests, pubPolicy);
            Task.WaitAll(t1);
            Console.WriteLine("PUB Authz policy added");

            AuthorizationPolicy subPolicy = CreateAuthZPolicy("sub", "http://www.skunklab.io/resource1/sub");
            Task t2 = AddAccessControlPolicy(tests, subPolicy);
            Task.WaitAll(t2);
            Console.WriteLine("SUB Authz policy added");

            Task<AuthorizationPolicy> t3 = GetAccessControlPolicy(pubPolicy.PolicyId.ToString(), tests);
            Task.WaitAll(t3);
            AuthorizationPolicy pubRetVal = t3.Result;
            if(pubRetVal.PolicyId.ToString() != pubPolicy.PolicyId.ToString())
            {
                Console.WriteLine("Pub Policy Get failed");
            }
            else
            {
                Console.WriteLine("Got Pub Policy");
            }


            Task<AuthorizationPolicy> t4 = GetAccessControlPolicy(subPolicy.PolicyId.ToString(), tests);
            Task.WaitAll(t4);
            AuthorizationPolicy subRetVal = t4.Result;

            if (subRetVal.PolicyId.ToString() != subPolicy.PolicyId.ToString())
            {
                Console.WriteLine("Sub Policy Get failed");
            }
            else
            {
                Console.WriteLine("Got Sub Policy");
            }

            ResourceMetadata resourceMetadata = new ResourceMetadata()
            {
                ResourceUriString = resourceUriString,
                Enabled = true,
                PublishPolicyUriString = pubPolicy.PolicyId.ToString(),
                SubscribePolicyUriString = subPolicy.PolicyId.ToString(),
                RequireEncryptedChannel = false
            };

            Task t5 = AddResourceMetadata(tests, resourceMetadata);
            Task.WaitAll(t5);
            Console.WriteLine("Add ResourceMetadata");

            Task<ResourceMetadata> t6 = GetResourceMetadata(tests, resourceUriString);
            Task.WaitAll(t6);
            ResourceMetadata rmRetVal = t6.Result;

            if(rmRetVal.ResourceUriString != resourceMetadata.ResourceUriString)
            {
                Console.WriteLine("ResourceMetadata Get failed");
            }
            else
            {
                Console.WriteLine("Got ResourceMetadata");
            }

            SubscriptionMetadata subMetadata = new SubscriptionMetadata()
            { 
                 IsEphemeral = true
            };

            Task<string> t7 = Subscribe(resourceUriString, subMetadata, tests);
            Task.WaitAll(t7);
            string subscriptionUriString = t7.Result;
            Console.WriteLine("Subscribed");

            Task t8 = Observe(subscriptionUriString, tests);

            EventMessage em = new EventMessage("text/plain", resourceUriString, ProtocolType.REST, Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString()));
            Task t9 = Publish(em, tests);
            Task.WaitAll(t9);
            Console.WriteLine("Published");

            Task t10 = Unsubscribe(subscriptionUriString, tests);
            Console.WriteLine("Unsubscribed");

            Console.WriteLine("Done!!!");
            Console.ReadKey();
        }

        private static AuthorizationPolicy CreateAuthZPolicy(string matchValue, string policyId)
        {
            Match match = new Match(Capl.Authorization.Matching.LiteralMatchExpression.MatchUri, "http://www.skunklab.io/piraeus/role", true);
            EvaluationOperation operation = new EvaluationOperation(Capl.Authorization.Operations.EqualOperation.OperationUri, matchValue);
            Rule rule = new Rule(match, operation, true);
            return new AuthorizationPolicy(rule, new Uri(policyId));
        }


        static async Task AddAccessControlPolicy(PrepTests tests, AuthorizationPolicy policy)
        {
            await tests.AddAccessControlPolicy(policy);
        }

        static async Task<AuthorizationPolicy> GetAccessControlPolicy(string policyId, PrepTests tests)
        {
            return await tests.GetAccessControlPolicy(policyId);
        }

        static async Task AddResourceMetadata(PrepTests tests, ResourceMetadata metadata)
        {
            await tests.AddResourceMetadata(metadata);
        }

        static async Task<ResourceMetadata> GetResourceMetadata(PrepTests tests, string resourceUriString)
        {
            return await tests.GetResourceMetadata(resourceUriString);
        }

        static async Task<string> Subscribe(string resourceUriString, SubscriptionMetadata metadata, PrepTests tests)
        {
            return await tests.Subscribe(resourceUriString, metadata);
        }

        static async Task Observe(string subscriptionUriString, PrepTests tests)
        {
            MessageObserver observer = new MessageObserver();
            observer.OnNotify += Observer_OnNotify;

            await tests.ObserveSubscription(subscriptionUriString, TimeSpan.FromMinutes(5.0), observer);
        }

        private static void Observer_OnNotify(object sender, MessageNotificationArgs e)
        {
            long ticksEnd = DateTime.Now.Ticks;
            long ticksStart = Int64.Parse(Encoding.UTF8.GetString(e.Message.Message));
            TimeSpan ts = TimeSpan.FromTicks(ticksEnd - ticksStart);
            Console.WriteLine("Observer Message Time --- {0} ---", ts.TotalMilliseconds);
        }

        public static async Task Publish(EventMessage message, PrepTests tests)
        {
            await tests.Publish(message.ResourceUri, message);
        }

        public static async Task Unsubscribe(string subscriptionUriString, PrepTests tests)
        {
            await tests.Unsubscribe(subscriptionUriString);
        }
    }
}
