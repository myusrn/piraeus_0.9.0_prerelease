using Piraeus.Configuration;
using Piraeus.Configuration.Settings;
using SkunkLab.Listeners;
using SkunkLab.Listeners.Tcp;
using SkunkLab.Listeners.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpUdpGatewayDocker.Configuration;

namespace TcpUdpGatewayDocker
{
    class Program
    {
        static CancellationTokenSource source;
        static Dictionary<int, TcpServerListener> tcpListeners;
        static Dictionary<int, UdpServerListener> udpListeners;
        static Dictionary<int, CancellationTokenSource> sources;
        static PiraeusConfig config;

        static void Main(string[] args)
        {
            tcpListeners = new Dictionary<int, TcpServerListener>();
            udpListeners = new Dictionary<int, UdpServerListener>();
            sources = new Dictionary<int, CancellationTokenSource>();
            config = PiraeusConfigManager.Settings;

            InitOrleansClient();

            if (!Orleans.GrainClient.IsInitialized)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Orleans client failed to initialize");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("press any key to terminate...");
                Console.ReadKey();
                return;
            }

            int[] tcpPorts = new int[] { 1883, 8883, 5684 };
            int[] udpPorts = new int[] { 5683, 5883 };

            foreach (var port in tcpPorts)
            {
                sources.Add(port, new CancellationTokenSource());
            }

            foreach (var port in udpPorts)
            {
                sources.Add(port, new CancellationTokenSource());
            }



            tcpListeners.Add(tcpPorts[0], new TcpServerListener(new IPEndPoint(GetIPAddress("localhost"), tcpPorts[0]), config, sources[tcpPorts[0]].Token));
            tcpListeners.Add(tcpPorts[1], new TcpServerListener(new IPEndPoint(GetIPAddress("localhost"), tcpPorts[1]), config, sources[tcpPorts[1]].Token));
            tcpListeners.Add(tcpPorts[2], new TcpServerListener(new IPEndPoint(GetIPAddress("localhost"), tcpPorts[2]), config, sources[tcpPorts[2]].Token));

            udpListeners.Add(tcpPorts[0], new UdpServerListener(config, new IPEndPoint(GetIPAddress("localhost"), udpPorts[0]), sources[udpPorts[0]].Token));
            udpListeners.Add(tcpPorts[1], new UdpServerListener(config, new IPEndPoint(GetIPAddress("localhost"), udpPorts[1]), sources[udpPorts[1]].Token));

            KeyValuePair<int, TcpServerListener>[] tcpKvps = tcpListeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                Task task = item.Value.StartAsync();
                Task.WhenAll(task);
            }

            KeyValuePair<int, UdpServerListener>[] udpKvps = udpListeners.ToArray();

            foreach (var item in udpKvps)
            {
                item.Value.OnError += Listener_OnError;
                Task task = item.Value.StartAsync();
                Task.WhenAll(task);
            }

            Console.ReadKey();

        }

        private static void Listener_OnError(object sender, ServerFailedEventArgs e)
        {
            if (sources.ContainsKey(e.Port))
            {
                //cancel the server
                sources[e.Port].Cancel();
            }
            else
            {
                return;
            }

            if (tcpListeners.ContainsKey(e.Port))
            {
                Task task = tcpListeners[e.Port].StopAsync();
                Task.WhenAll(task);
                tcpListeners.Remove(e.Port);
                sources.Remove(e.Port);

                //restart the server
                sources.Add(e.Port, new CancellationTokenSource());
                tcpListeners.Add(e.Port, new TcpServerListener(new IPEndPoint(GetIPAddress("localhost"), e.Port), config, sources[e.Port].Token));


            }

            if (udpListeners.ContainsKey(e.Port))
            {
                Task task = udpListeners[e.Port].StopAsync();
                Task.WhenAll(task);
                udpListeners.Remove(e.Port);
                sources.Remove(e.Port);

                //restart the server
                sources.Add(e.Port, new CancellationTokenSource());
                udpListeners.Add(e.Port, new UdpServerListener(config, new IPEndPoint(GetIPAddress("localhost"), e.Port), sources[e.Port].Token));
            }





        }

        static void InitOrleansClient()
        {


            int max = 6; //try for 1 minute
            int index = 0;

            while (index < max)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Initializing Orleans client...");
                Console.ResetColor();

                if(OrleansClientConfig.TryStart("TCPUDP-Gateway"))
                {
                    break;
                }
                else
                {
                    Thread.Sleep(10000);
                    index++;
                }
            }
        }

        static IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }
    }
}
