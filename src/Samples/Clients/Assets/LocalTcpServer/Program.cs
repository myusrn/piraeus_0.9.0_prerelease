using Piraeus.Configuration;
using Piraeus.Configuration.Settings;
using SkunkLab.Servers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LocalTcpServer
{
    class Program
    {
        static CancellationTokenSource source;
        static Dictionary<string, string> cliInput;
        static IPAddress address;
        static List<int> ports;
        static List<TcpServerListener> listeners;
        static void Main(string[] args)
        {
            List<string> switches = new List<string>(new string[]{ "-h", "-p" });
            Parser.Switches = switches;

            cliInput = Parser.Parse(args);
            if(!CheckInput())
            {
                WriteHelp();
                return;
            }

           
            InitOrleansClient();
            

            WriteHeader();            

            if (!Orleans.GrainClient.IsInitialized)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Orleans client failed to initialize");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("press any key to terminate...");
                Console.ReadKey();
                return;
            }
            
            source = new CancellationTokenSource();
            PiraeusConfig config = PiraeusConfigManager.Settings;
            

            Console.WriteLine("TCP Servers starting...");

            listeners = new List<TcpServerListener>();
            foreach(int port in ports)
            {
                TcpServerListener listener = new TcpServerListener(address, port, config, source.Token);
                Task task = listener.StartAsync();
                Task.WhenAll(task);
                listeners.Add(listener);
            }

            
            Console.WriteLine("Press any key to terminate.");

            Console.ReadKey();
            source.Cancel();
            Thread.Sleep(5000);


            
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


        static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Local TCP Server ---");
            Console.Write("IP Address : {0} ", address.ToString());
            Console.Write("Ports : ");
            for (int i=0;i<ports.Count;i++)
            {
                Console.Write("{0} ", ports[i]);
            }
            Console.WriteLine("--------------------------");
            Console.ResetColor();
            

        }
        static void WriteHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Invalid command line !");
            Console.WriteLine("LocalTcpServer -h <hostname or host ip> -p <port1,port2,...>");
            Console.ResetColor();
            Console.ReadKey();
        }
        

        static bool CheckInput()
        {
            ports = new List<int>();
            if (cliInput.ContainsKey("-h") && cliInput.ContainsKey("-p"))
            {
                if (!IPAddress.TryParse(cliInput["-h"], out address))
                {
                    address = GetIPAddress(cliInput["-h"]);
                    if(address == null)
                    {
                        return false;
                    }                    
                }

                string[] parts = cliInput["-p"].Split(new char[] { ',' });
                if (parts != null && parts.Length > 0)
                {
                    foreach (string part in parts)
                    {
                        int port = -1;
                        if (Int32.TryParse(part, out port))
                        {
                            ports.Add(port);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        static void InitOrleansClient()
        {
            

            int max = 3;
            int index = 0;

            while (index < max)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Initializing Orleans client...");
                Console.ResetColor();

                try
                {
                    var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                    Orleans.GrainClient.Initialize(config);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Orleans client started");
                    Console.ResetColor();
                    break;
                }
                catch (Exception ex)
                {              
                    index++;

                }
            }
        }
    }
}
