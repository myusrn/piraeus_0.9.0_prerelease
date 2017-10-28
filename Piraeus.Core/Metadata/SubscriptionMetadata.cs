using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piraeus.Core.Metadata
{
    [Serializable]
    [JsonObject]
    public class SubscriptionMetadata
    {
        public SubscriptionMetadata()
        {

        }

        public SubscriptionMetadata(string identity, string subscriptionUriString, string address, string symmetricKey, TimeSpan? ttl = null, DateTime? expires = null, TimeSpan? spoolRate = null, bool durableMessaging = false)
        {
            Identity = identity;
            SubscriptionUriString = subscriptionUriString;
            NotifyAddress = address;
            SymmetricKey = symmetricKey;
            TTL = ttl;
            Expires = expires;
            SpoolRate = spoolRate;
            DurableMessaging = durableMessaging;
            IsEphemeral = false;
        }

        public SubscriptionMetadata(string subscriptionUriString)
        {
            SubscriptionUriString = subscriptionUriString;
            IsEphemeral = true;
        }


        /// <summary>
        /// Gets or sets the identity name claim value for a durable subscription.
        /// </summary>
        /// <remarks>This allows the durable subscription to be observed when the subsystem connects automatically.  The identity is required
        /// for a durable subscription where the subsystem actively connects.  It is omitted for a passively connected subsystem or an ephemeral subscription.</remarks>       
        [JsonProperty("identity")]
        public string Identity { get; set; }

        /// <summary>
        /// Gets of sets the indicator whether the subscription is durable or ephemeral.
        /// </summary>
        /// <remarks>An ephemeral subscription cannot be provisioned through the management API.</remarks>
        [JsonProperty("isEphemeral")]
        public bool IsEphemeral { get; set; }

        /// <summary>
        /// Gets or sets the subscription's URI that uniquely identifies the subscription.
        /// </summary>
        [JsonProperty("subscriptionUriString")]
        public string SubscriptionUriString { get; set; }

       
        /// <summary>
        /// Gets or sets the time To live of a message in the subscription
        /// </summary>
        [JsonProperty("ttl")]
        public TimeSpan? TTL { get; set; }

        /// <summary>
        /// Gets or sets the endpoint address of a passive service.  
        /// </summary>
        [JsonProperty("notifyAddress")]
        public string NotifyAddress { get; set; }

        /// <summary>
        /// Gets or sets the expiration of the subscription.  If omitted, the subscription expiration is infinite unless the subscription is ephemeral.
        /// </summary>
        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }


        /// <summary>
        /// Get or set an optional security token type to send in a request to a passive receiving subsystem. 
        /// </summary>
        [JsonProperty("securityTokenType")]
        public SecurityTokenType? TokenType { get; set; }


        [JsonProperty("indexes")]
        public List<KeyValuePair<string,string>> Indexes { get; set; }

        /// <summary>
        /// Gets or sets an optional symmetric key when the security token type is set to JWT or SWT.
        /// </summary>
        [JsonProperty("symmetricKey")]
        public string SymmetricKey { get; set; }


        /// <summary>
        /// Gets or sets the rate in milliseconds that a retained message is sent after the subsystem has reconnected.
        /// </summary>
        [JsonProperty("spoolRate")]
        public TimeSpan? SpoolRate { get; set; }

        [JsonProperty("durableMessaging")]
        public bool DurableMessaging { get; set; }
    }
}
