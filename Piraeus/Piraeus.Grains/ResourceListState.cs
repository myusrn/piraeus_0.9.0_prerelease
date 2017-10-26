using System;
using System.Collections.Generic;

namespace Piraeus.Grains
{
    [Serializable]
    public class ResourceListState
    {
        public List<string> Container { get; set; }
    }
}
