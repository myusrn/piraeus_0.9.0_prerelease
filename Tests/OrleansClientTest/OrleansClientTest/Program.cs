using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Core.Metadata;
using Piraeus.ServiceModel;

namespace OrleansClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Orleans Client Test...");
            Console.ReadKey();

            Console.WriteLine("Initializing Orleans Client");
            try
            {
                Init();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Olreans client initialized");

            Console.WriteLine("Add some metadata");
            try
            {
                AddMetadata();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Metadata addded");

            Console.WriteLine("Get metadata");
            try
            {
                GetMetadata();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Got metadata");
            Console.WriteLine("Done");
            Console.ReadKey();


        }

        static void GetMetadata()
        {
            try
            {
                Task<ResourceMetadata> task = GraphManager.GetResourceMetadataAsync("http://www.example.org/resource1");
                Task.WhenAll<ResourceMetadata>(task);
                ResourceMetadata metadata = task.Result;
            }
            catch(AggregateException ae)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ae.Flatten().InnerException.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
        }
        static void AddMetadata()
        {
            ResourceMetadata metadata = new ResourceMetadata()
            {
                RequireEncryptedChannel = false,
                Enabled = true,
                PublishPolicyUriString = "http://www.example.org/pubpolicy",
                ResourceUriString = "http://www.example.org/resource1",
                SubscribePolicyUriString = "http://www.example.org/subpolicy"
            };

           
            Task task = GraphManager.AddResourceAsync(metadata);
            Task.WaitAll(task);
        }

        static void Init()
        {
            var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
            Orleans.GrainClient.Initialize(config);
        }
    }
}
