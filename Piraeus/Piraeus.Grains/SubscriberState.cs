using System;
using System.Collections.Generic;

namespace Piraeus.Grains
{
    [Serializable]
    public class SubscriberState
    {
        public List<string> Container { get; set; }
    }
}
