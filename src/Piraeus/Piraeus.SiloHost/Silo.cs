using System;
using System.Configuration;
using System.Net;
using Orleans.Runtime.Configuration;
using System.Linq;
using System.Threading;
using System.Diagnostics;

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

            if (dockerized)
            {
                //USE for production and clustering
                config = new ClusterConfiguration();
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
                //USE for demo or local test
                config = ClusterConfiguration.LocalhostPrimarySilo();
                config.AddMemoryStorageProvider("store", 1000);
            }

            string hostname = System.Net.Dns.GetHostName();
            Console.WriteLine("Hostname of silo '{0}'", hostname);

            var siloHost = new Orleans.Runtime.Host.SiloHost(hostname, config);
            Console.WriteLine("Silo host initialized.");

            hostWrapper = new OrleansHostWrapper(config, args);
            Console.WriteLine("Running host wrapper.");
            return hostWrapper.Run();
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
