using System;
using System.Configuration;
using System.Net;
using Orleans.Runtime.Configuration;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Orleans.Storage.Redis;
using System.Collections.Generic;

namespace Piraeus.SiloHost
{
    public class Silo
    {
        private static OrleansHostWrapper hostWrapper;
        private static bool dockerized;


        public static int Run(string[] args)
        {
            dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            return StartSilo(args);
        }
               

        private static int StartSilo(string[] args)
        {
            // define the cluster configuration   
            ClusterConfiguration config = null;
            string hostname = System.Net.Dns.GetHostName();
            
            if (dockerized)
            {

                //USE for production and clustering
                config = new ClusterConfiguration();
                config.Globals.DataConnectionString = System.Environment.GetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING");
                config.Globals.DataConnectionStringForReminders = System.Environment.GetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING");
                config.Globals.DeploymentId = System.Environment.GetEnvironmentVariable("ORLEANS_DEPLOYMENT_ID");
                config.Globals.LivenessEnabled = true;
                config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;               
                config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
                

                config.Defaults.PropagateActivityId = true;
                config.Defaults.HostNameOrIPAddress = hostname;
                config.Defaults.Port = 11111;
                config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 30000);
                config.Defaults.SiloName = hostname;

                ServicePointManager.DefaultConnectionLimit = 12 * Environment.ProcessorCount;

                IDictionary<string, string> properties = GetStorageProviderProperties();

                string providerName = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE").ToLowerInvariant();
                if (providerName == "azureblobstore")
                {
                    config.Globals.RegisterStorageProvider<Orleans.Storage.AzureBlobStorage>("store", properties);
                }
                else if (providerName == "redisstore")
                {
                    config.Globals.RegisterStorageProvider<RedisStorageProvider>("store", properties);
                }
                else if (providerName == "memorystore")
                {
                    config.Globals.RegisterStorageProvider<Orleans.Storage.MemoryStorage>("store", properties);
                }
                else
                {
                    Console.WriteLine("Orleans Storage Provider NAME not understood.");
                    throw new ArgumentOutOfRangeException("Provider name is not recognized for Orleans storage provider.");
                }


                try
                {
                    config.LoadFromFile("OrleansConfiguration.xml");
                    Console.WriteLine("Configuration file loaded.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Failed to load config file with {0}", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Identified as localhost deployment.");
                //USE for demo or local testing
                config = ClusterConfiguration.LocalhostPrimarySilo();
                config.AddMemoryStorageProvider("store", 1000);
            }


            var siloHost = new Orleans.Runtime.Host.SiloHost(hostname, config);
            Console.WriteLine("Silo host initialized.");

            hostWrapper = new OrleansHostWrapper(config, args);
            Console.WriteLine("Starting host wrapper run.");
            return hostWrapper.Run();
        }

        private static IDictionary<string,string> GetStorageProviderProperties()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string providerName = System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE").ToLowerInvariant();

            if (providerName == "azureblobstore")
            {
                dict.Add("ContainerName", System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME"));
                dict.Add("DataConnectionString", System.Environment.GetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING"));
            }
            if(providerName == "redisstore")
            {
                dict.Add("DataConnectionString", System.Environment.GetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING"));
            }

            if(providerName == "memorystore")
            {
                dict.Add("NumStorageGrains", System.Environment.GetEnvironmentVariable("ORLEANS_MAXMEMORY_STORAGE_GRAINS"));
            }
            

            return dict;
        }

        private static int ShutdownSilo()
        {
            Console.WriteLine("Shutdown Hostwrapper");
        
            if (hostWrapper != null)
            {
                return hostWrapper.Stop();
            }
            return 0;
        }
    }
}
