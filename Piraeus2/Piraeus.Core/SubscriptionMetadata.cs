using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;

namespace Piraeus.Core
{
    [Serializable]
    [JsonObject]
    public class SubscriptionMetadata
    {
        public SubscriptionMetadata()
        {
            Indexes = new List<KeyValuePair<string, string>>();
        }

        [JsonProperty("id")]
        public string Id { get; set; }        

        [JsonProperty("empheral")]
        public bool IsEphemeral { get; set; }

        /// <summary>
        /// Gets or sets the subscription's URI that uniquely identifies the subscription.
        /// </summary>
        
        [JsonProperty("subscriptionUri")]
        public string SubscriptionUri { get; set; }

        [JsonProperty("indexes")]
        public List<KeyValuePair<string,string>> Indexes { get; set; }
        /// <summary>
        /// Gets or sets the Time To Live of a message in the subscription
        /// </summary>

        [JsonProperty("ttl")]
        public TimeSpan? TTL { get; set; }

        /// <summary>
        /// Gets or sets the endpoint address of Web service.  If omitted, the subscription message is passed via a Web Socket.
        /// </summary>
        [JsonProperty("notifyAddress")]
        public string NotifyAddress { get; set; }

        /// <summary>
        /// Gets or sets the expiration of the subscription.  If omitted, the subscription expiration is infinite.
        /// </summary>
        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }
        
        /// <summary>
        /// Get or set an optional security token type. 
        /// </summary>
        [JsonProperty("tokenType")]
        public SecurityTokenType? TokenType { get; set; }


        /// <summary>
        /// Gets or sets an optional symmetric key when the security token type is set to JWT or SWT.
        /// </summary>
        [JsonProperty("symmetricKey")]
        public string SymmetricKey { get; set; }


        [JsonProperty("spoolRate")]
        public TimeSpan? SpoolRate { get; set; }
    }


    public enum SecurityTokenType
    {
        None,
        Jwt,
        Swt,
        X509
    }
}
