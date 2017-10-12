using System;

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
