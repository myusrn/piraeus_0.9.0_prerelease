using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Core
{
    public class Lease
    {
        public Lease()
        {

        }

        public string Key { get; set; }

        public TimeSpan Duration { get; set; }

    }
}
