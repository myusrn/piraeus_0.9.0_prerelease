using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piraeus.Core.Metadata;
using Piraeus.Grains;

namespace PiraeusUnitTest
{
    [TestClass]
    public class ResourceTests
    {
        private const string RESOURCE_URI = "http://www.example.org/resource1";
        private const string IDENTITY = "myidentity";
        private Process process;
        [TestInitialize]
        public void Init()
        {
            ProcessStartInfo psi = new ProcessStartInfo(@"C:\_git\core\Junk\TestHost\bin\debug\TestHost.exe");
            process = Process.Start(psi);
            //wait for silo process to be ready
            Thread.Sleep(10000);

            //start the Orleans client, then exit

            while (!Orleans.GrainClient.IsInitialized)
            {
                var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                Orleans.GrainClient.Initialize(config);
                Thread.Sleep(2000);
            }

            Console.WriteLine("Orleans client activated");
        }
        [TestCleanup]
        public void Cleanup()
        {
            if(process != null)
            {
                process.Kill();
            }
        }

        #region Get and Add Resource Metadata
        [TestMethod]
        public void AddPlusGetResourceMetadataTest()
        {
            try
            {
                Task task = AddPlusGetResourceMetadataTestAsync();
                Task.WaitAll(task);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddPlusGetResourceMetadataTestAsync()
        {
            ResourceMetadata actual = new ResourceMetadata()
            {
                RequireEncryptedChannel = false,
                Enabled = true,
                PublishPolicyUriString = "http://www.example.org/pubpolicy",
                ResourceUriString = RESOURCE_URI,
                SubscribePolicyUriString = "http://www.example.org/subpolicy"
            };

            await GraphManager.UpsertResourceMetadataAsync(actual);

            ResourceMetadata expected = await GraphManager.GetResourceMetadata(actual.ResourceUriString);

            Assert.AreEqual(expected.ResourceUriString, actual.ResourceUriString);
        }

        #endregion

        [TestMethod]
        public void SubscribeToResourceTest()
        {
            Task task = SubscribeToResourceTestAsync();
            Task.WaitAll(task);
        }

        public async Task SubscribeToResourceTestAsync()
        {
            string resourceUriString = "http://www.example.org/resource1";
            string identity = "myidentity";

            SubscriptionMetadata metadata = GetSubscriptionMetadataLocal(identity, true);
            string uriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);
            Assert.IsNotNull(uriString);

        }

        private SubscriptionMetadata GetSubscriptionMetadataLocal(string identity, bool ephemeral)
        {
            return new SubscriptionMetadata()
            {
                Identity = identity,
                IsEphemeral = ephemeral
            };
        }

    }
}
