using Piraeus.Configuration;
using Piraeus.Configuration.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Servers.Tcp
{
    class Program
    {
        static CancellationTokenSource source;
        static void Main(string[] args)
        {
            InitOrleansClient();
            source = new CancellationTokenSource();
            PiraeusConfig config = PiraeusConfigManager.Settings;
            
            TcpServerListener listener = new TcpServerListener(IPAddress.Parse(args[0]), Int32.Parse(args[1]), config, source.Token);
            Task task = listener.StartAsync();
            Task.WaitAll(task);
        }

        static void InitOrleansClient()
        {
            var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
            Orleans.GrainClient.Initialize(config);
        }
    }
}
