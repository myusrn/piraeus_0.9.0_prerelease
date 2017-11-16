using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SkunkLab.Common
{
    [JsonObject("securityKey")]
    public class SecurityKey
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("lifetimeMinutes")]
        public int LifetimeMinutes { get; set; }

        [JsonProperty("claims")]
        public ClaimLiteral[] Claims { get; set; }
    }
}
