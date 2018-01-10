using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;

namespace WebGateway.Security
{
    public class OrleansClientConfig
    {
        public static bool TryStart(string location)
        {

            if (Orleans.GrainClient.IsInitialized)
                return true;

            bool started = false;
            Orleans.Runtime.Configuration.ClientConfiguration config = null;
            string faultMessage = null;
            string value = ConfigurationManager.AppSettings["dockerized"];
            bool dockerized = false;

            if (bool.TryParse(value, out dockerized))
            {
                string dockerizedString = dockerized ? "Dockerized" : "Non-dockerized";
                faultMessage = String.Format("{0} Web Gateway failed to initialize Orleans client in {1}.", dockerizedString, location);
                
                if(dockerized)
                {
                    var hostEntry = Dns.GetHostEntry("orleans-silo");
                    var ip = hostEntry.AddressList[0];
                    config = new Orleans.Runtime.Configuration.ClientConfiguration();
                    config.Gateways.Add(new IPEndPoint(ip, 30000));
                }
                else
                {
                    config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                }

                try
                {
                    Orleans.GrainClient.Initialize(config);
                    started = true;
                }
                catch
                {
                    Trace.TraceWarning(faultMessage);
                }
            }
            else
            {
                faultMessage = String.Format("Orleans client cannot start because 'dockerized' setting is not configured.");
            }

            return started;
        }
    }
}