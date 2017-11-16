using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Common
{

    [JsonObject("claimLiteral")]
    public class ClaimLiteral
    {
        public ClaimLiteral()
        {
        }

        /// <summary>
        /// Claim Type that identifies the claim.
        /// </summary>
        [JsonProperty("claimType")]
        public string ClaimType { get; set; }

        /// <summary>
        /// Value of the claim.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
