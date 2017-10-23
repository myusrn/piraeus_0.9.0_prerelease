using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Configuration;

namespace ConfigAllTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Piraeus.Configuration.Settings.PiraeusConfig config = PiraeusConfigManager.Settings;
        }
    }
}
