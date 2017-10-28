using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
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
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            Task task = Task.Factory.StartNew(async () =>
            {
                //await AddAsync();
                await GetAsync();
            });

            Task.WaitAll(task);

        
            Console.WriteLine("Done");
            Console.ReadKey();


        }

        static async Task AddAsync()
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
            catch(AggregateException ae)
            {
                Console.WriteLine("AE ADD {0}", ae.Flatten().InnerException.Message);

            }
            catch(Exception ex)
            {
                Console.WriteLine("EX ADD {0}", ex.Message);

            }
        }

        static async Task GetAsync()
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
        

        static void Init()
        {
            var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
            Orleans.GrainClient.Initialize(config);
        }
    }
}
