using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text;
using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using AdapterTests.Settings;
using SkunkLab.Channels;
using SkunkLab.Channels.Http;
using System.Collections.Generic;
using System.IO;
using Piraeus.Grains;
using Piraeus.Core.Metadata;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web;
using System.Linq;

namespace AdapterTests
{
    [TestClass]
    public class PiraeusAdapterTests
    {
        private static string resourceUriString = "http://www.skunklab.io/resource1";

        [TestInitialize]
        public void Init()
        {
            while (!Orleans.GrainClient.IsInitialized)
            {
                var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                Orleans.GrainClient.Initialize(config);
                Thread.Sleep(2000);
            }


            Capl.Authorization.Match match = new Capl.Authorization.Match(Capl.Authorization.Matching.LiteralMatchExpression.MatchUri, "http://www.skunklab.io/role", true);
            Capl.Authorization.EvaluationOperation pubOperation = new Capl.Authorization.EvaluationOperation(Capl.Authorization.Operations.EqualOperation.OperationUri, "pub");
            Capl.Authorization.Rule pubRule = new Capl.Authorization.Rule(match, pubOperation, true);
            Capl.Authorization.AuthorizationPolicy pubPolicy = new Capl.Authorization.AuthorizationPolicy(pubRule, new Uri("http://www.skunklab.io/resource1/sub-policy"));

            Capl.Authorization.EvaluationOperation subOperation = new Capl.Authorization.EvaluationOperation(Capl.Authorization.Operations.EqualOperation.OperationUri, "pub");
            Capl.Authorization.Rule subRule = new Capl.Authorization.Rule(match, subOperation, true);
            Capl.Authorization.AuthorizationPolicy subPolicy = new Capl.Authorization.AuthorizationPolicy(pubRule, new Uri("http://www.skunklab.io/resource1/sub-policy"));

            Task pubAuthzTask = GraphManager.UpsertAcessControlPolicyAsync(pubPolicy.PolicyId.ToString(), pubPolicy);
            Task.WaitAll(pubAuthzTask);
            Task subAuthzTask = GraphManager.UpsertAcessControlPolicyAsync(subPolicy.PolicyId.ToString(), subPolicy);
            Task.WaitAll(subAuthzTask);

            ResourceMetadata metadata = new ResourceMetadata()
            {
                Enabled = true,
                PublishPolicyUriString = pubPolicy.PolicyId.ToString(),
                SubscribePolicyUriString = subPolicy.PolicyId.ToString(),
                ResourceUriString = resourceUriString
            };

            GraphManager.UpsertResourceMetadataAsync(metadata);

            Task task  = GraphManager.UpsertResourceMetadataAsync(metadata);
            Task.WaitAll(task);
        }


        /// <summary>
        /// Http long-polling receiver. will subscribe to a resource.
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            PiraeusConfig config = TestConfigSettings.GetDefaultConfig();
            //claims for token
            List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>() { new System.Security.Claims.Claim("http://www.skunklab.io/name", "testuser"), new System.Security.Claims.Claim("http://www.skunklab.io/role", "sub") };
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(config.Security.Client.Audience), config.Security.Client.SymmetricKey, config.Security.Client.Issuer, claims);
            HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, "http://www.example.org");
            StringContent sc = new StringContent("hello", Encoding.UTF8, "text/plain");
            sc.Headers.Add("x-sl-subscribe", resourceUriString);
            sc.Headers.Add("Authorization", String.Format("Bearer {0}", jwt.ToString()));
            hrm.Content = sc;

            HttpWorkerRequest wr = new SimpleWorkerRequest("/api/connect", "", "", "", TextWriter.Null);

            HttpContext.Current = new HttpContext(wr);
            IChannel channel = new HttpServerChannel(hrm);
            RestProtocolAdapter restProtocolAdapter = new RestProtocolAdapter(config, channel);
            restProtocolAdapter.Init();

            
                
                //should be able to get a subscription list from the GraphManager and verify subscription



            


           
        }
    }
}
