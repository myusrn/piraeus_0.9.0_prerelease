using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SkunkLab.Common
{

    [JsonObject("topology")]
    public class Topology
    {
        [JsonProperty("topic")]
        public TopicMetadata Topic { get; set; }

        [JsonProperty("subscriptions")]
        public SubscriptionMetadata[] Subscriptions { get; set; }
    }
}
