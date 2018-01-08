using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri addressUri = new Uri("https://storagesample.blob.core.windows.net/container");
            if(addressUri.LocalPath.Length > 1)
            {
                Console.WriteLine(addressUri.LocalPath.Replace("/", ""));
                Console.WriteLine(addressUri.ToString().Replace(addressUri.LocalPath, ""));
            }

            Console.ReadKey();

        }
    }
}
