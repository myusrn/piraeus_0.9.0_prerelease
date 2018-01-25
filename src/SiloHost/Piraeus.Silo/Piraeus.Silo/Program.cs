using System;
using System.Configuration;
using System.Threading;

namespace Piraeus.Silo
{
    class Program
    {
        static int Main(string[] args)
        {
            int code = -1;

            try
            {
                code = Piraeus.SiloHost.Silo.Run(args);

                bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                Console.WriteLine("Piraeus silo dockerized {0}", dockerized);

                if (!dockerized)
                {
                    if (code == 0)
                    {
                        Console.WriteLine("Press any key to terminate...");
                        Console.ReadLine();
                    }
                }
                else
                {
                    if (code == 0)
                    {
                        ManualResetEventSlim running = new ManualResetEventSlim();
                        Console.WriteLine("Orleans silo is running on docker...");

                        Console.CancelKeyPress += (sender, eventArgs) =>
                        {
                            running.Set();
                            eventArgs.Cancel = true;
                        };

                        Console.WriteLine("Piraeus silo is running and blocking.");
                        running.Wait();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Piraeus silo threw exception {0}", ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return code;

        }
    }
}
