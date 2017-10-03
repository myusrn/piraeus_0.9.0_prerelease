using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Servers;

namespace CoapServer
{
    public class TcpServer
    {
        public TcpServer(IPEndPoint ip, CancellationToken token)
        {
            listener = new TcpServerListener(ip, token);
        }

        private TcpServerListener listener;

        public async Task RunAsync()
        {
            await listener.StartAsync();
        }

        public async Task StopAsync()
        {
            await listener.StopAsync();
        }


    }
}
