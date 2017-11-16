using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SkunkLab.Common
{
    [Serializable]
    [JsonObject("topicMetadata")]
    public class TopicMetadata
    {
        public TopicMetadata()
        {
        }

        /// <summary>
        /// Gets or sets the virtual topic.
        /// </summary>
        //[Key]
        [JsonProperty("topicUri")]
        public string TopicUri { get; set; }

        /// <summary>
        /// Gets or sets a description about the topic.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a Url that provides information about the topic and the message structure.
        /// </summary>
        [JsonProperty("discoveryUrl")]
        public string DisoveryUrl { get; set; }

        /// <summary>
        /// Gets or sets an indicator as to whether the topic is enabled (able to receive messages) or not.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets an indicator as to whether the topics and subscriptions require an encrypted channel to get or send messages.
        /// </summary>
        [JsonProperty("requireSsl")]
        public bool RequireSsl { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration of a subscription.  If omitted the duration is infinite.
        /// </summary>
        [JsonProperty("maxSubscriptionDuration")]
        public TimeSpan? MaxSubscriptionDuration { get; set; }


        /// <summary>
        /// Gets or sets the expiration of the topic.  If omitted the topic never expires.
        /// </summary>
        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }


        ////Gets or sets the Time-To-Live for a message in a subscription.
        //public TimeSpan MessageTTL { get; set; }

        /// <summary>
        /// Gets or sets message tracking.
        /// </summary>
        [JsonProperty("enableAudit")]
        public bool EnableAudit { get; set; }

        ///// <summary>
        ///// Gets or sets the URI of the authorization policy for the topic.
        ///// </summary>
        [JsonProperty("topicAuthzPolicyId")]
        public string TopicAuthorizationPolicyUri { get; set; }

        ///// <summary>
        ///// Gets or sets the URI of the authorization policy for a subscription.
        ///// </summary>
        [JsonProperty("subscriptionAuthZPolicyId")]
        public string SubscriptionAuthorizationPolicyUri { get; set; }




    }
}
