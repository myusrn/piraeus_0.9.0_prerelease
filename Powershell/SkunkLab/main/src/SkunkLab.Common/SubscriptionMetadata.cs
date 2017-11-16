using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SkunkLab.Common
{
    [Serializable]
    [JsonObject]
    public class SubscriptionMetadata
    {
        public SubscriptionMetadata()
        {
        }

        public SubscriptionMetadata(Uri uri, string deviceId, TimeSpan? ttl, DateTime? expires)
        {
            this.SubscriptionUri = uri.ToString();
            this.DeviceId = deviceId;
            this.TTL = ttl;
            this.Expires = expires;
        }

        public SubscriptionMetadata(Uri uri, string deviceId, TimeSpan? ttl, DateTime? expires, Uri notifyAddress, SecurityTokenType? tokenType, string symmetricKey)
        {
            this.TTL = ttl;

            if (tokenType.HasValue)
            {
                if (symmetricKey != null && (tokenType.Value != SecurityTokenType.JWT || tokenType.Value != SecurityTokenType.SWT))
                {
                    throw new ArgumentOutOfRangeException("Symmetric key requires JWT or SWT token type.");
                }
            }
            else
            {
                if (symmetricKey == null)
                {
                    throw new ArgumentNullException("symmetricKey");
                }
            }

            this.SubscriptionUri = uri.ToString();
            this.DeviceId = deviceId;
            this.Expires = expires;
            this.NotifyAddress = notifyAddress.ToString();
            this.TokenType = tokenType;
            this.SymmetricKey = symmetricKey;
        }


        [JsonProperty("id")]
        public string DeviceId { get; set; }

        [JsonProperty("ephemeral")]
        public bool IsEphemeral { get; set; }

        /// <summary>
        /// Gets or sets the subscription's URI that uniquely identifies the subscription.
        /// </summary>
        [JsonProperty("subscriptionUri")]
        public string SubscriptionUri { get; set; }

        /// <summary>
        /// Gets or sets an optional key used in filtering/routing of subscriptions
        /// </summary>
        [JsonProperty("primaryKey")]
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Gets of sets an optional key used in filtering/routing of subscriptions
        /// </summary>
        [JsonProperty("secondaryKey")]
        public string SecondaryKey { get; set; }

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
}
