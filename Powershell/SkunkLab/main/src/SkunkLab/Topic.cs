using SkunkLab.Common;
using System;
using System.Management.Automation;

namespace SkunkLab
{
    /// <summary>
    /// Adds a topic 
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Topic")]
    public class AddTopic : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The Topic URI that uniquely identifies the topic.  The namespace of the topic must reside in namespace of the account.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "(Optional) Description of the topic.", Mandatory = false)]
        public string Description;

        [Parameter(HelpMessage = "(Optional) Url where more information can be found about the topic.", Mandatory = false)]
        public string DiscoveryUrl;

        [Parameter(HelpMessage = "Enables or disables the topic for communication.", Mandatory = true)]
        public bool Enabled;

        [Parameter(HelpMessage = "Requires the topic and its subscriptions to use an encrypted channel if True; otherwise an encrypted channel is not required for communications.", Mandatory = true)]
        public bool RequireSsl;

        [Parameter(HelpMessage = "(Optional) Maximum duration for a subscription.  If omitted a subscription can be of infinite duration.", Mandatory = false)]
        public TimeSpan? MaxSubscriptionDuration;

        [Parameter(HelpMessage = "(Optional) Expiration of the topic. If omitted the topic never expires.", Mandatory = false)]
        public DateTime? Expires;

        [Parameter(HelpMessage = "Enables or disables auditing of the topic.", Mandatory = true)]
        public bool EnableAudit;

        [Parameter(HelpMessage = "URI of the Access Control Policy Id for sending events to the topic.", Mandatory = true)]
        public string TopicAccessPolicyId;

        [Parameter(HelpMessage = "URI of the Access Control Policy Id for subscribing and receiving events from the topic.", Mandatory = true)]
        public string SubscriptionAccessPolicyId;

        protected override void ProcessRecord()
        {
            TopicMetadata metadata = new TopicMetadata()
            {
                TopicUri = this.Topic,
                Description = this.Description,
                DisoveryUrl = this.DiscoveryUrl,
                Enabled = this.Enabled,
                RequireSsl = this.RequireSsl,
                MaxSubscriptionDuration = this.MaxSubscriptionDuration,
                Expires = this.Expires,
                EnableAudit = this.EnableAudit,
                TopicAuthorizationPolicyUri = this.TopicAccessPolicyId,
                SubscriptionAuthorizationPolicyUri = this.SubscriptionAccessPolicyId
            };


        }
    }

    /// <summary>
    /// Removes a topic and all of its subscriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Topic")]
    public class RemoveTopic : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public string Topic;

        protected override void ProcessRecord()
        {
        }
    }

    /// <summary>
    /// Updates the metadata for a topic.
    /// </summary>
    [Cmdlet(VerbsData.Update, "Topic")]
    public class UpdateTopic : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public TopicMetadata Metadata;

        protected override void ProcessRecord()
        {
        }
    }

    /// <summary>
    /// Returns the metadata for a topic.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Topic")]
    public class GetTopic : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
        }
    }


    /// <summary>
    /// Returns all the topics associated with the account.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TopicList")]
    public class GetTopicList : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
        }
    }

}
