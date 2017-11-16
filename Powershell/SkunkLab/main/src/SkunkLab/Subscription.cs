using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using SkunkLab.Common;

namespace SkunkLab
{
    /// <summary>
    /// Adds a durable subscription to a topic and returns the subscription URI.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Subscription")]
    public class AddSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The unique name identifier associated with a claim.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "(Optional) Primary key associated with a claim.", Mandatory = false)]
        public string PrimaryKey;

        [Parameter(HelpMessage = "(Optional) Secondary key associated with a claim.", Mandatory = false)]
        public string SecondaryKey;

        [Parameter(HelpMessage = "(Optional) Time-To-Live for an event waiting to egress.  If omitted TTL = 0.", Mandatory = false)]
        public TimeSpan? TTL;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;

        [Parameter(HelpMessage = "(Optional) Rate at which queued events egress.  If omitted implies as fast as possible.", Mandatory = false)]
        public TimeSpan? SpoolRate;

        protected override void ProcessRecord()
        {
            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                DeviceId = this.Id,
                PrimaryKey = this.PrimaryKey,
                SecondaryKey = this.SecondaryKey,
                TTL = this.TTL,
                Expires = this.Expires,
                SpoolRate = this.SpoolRate
            };

        }
    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to Document DB.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DocumentDBSubscription")]
    public class AddDocumentDBSubscription : Cmdlet
    {

        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The unique identifier associated with the subscription.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "Document DB account name.", Mandatory = true)]
        public string AccountName;

        [Parameter(HelpMessage = "Document DB database name.", Mandatory = true)]
        public string Database;

        [Parameter(HelpMessage = "Document DB collection name.", Mandatory = true)]
        public string Collection;

        [Parameter(HelpMessage = "Symmetric Key of the Document DB account.", Mandatory = true)]
        public string SymmetricKey;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;

        protected override void ProcessRecord()
        {
            SubscriptionMetadata metadata = new SubscriptionMetadata();
            metadata.NotifyAddress = String.Format("https://{0}.documents.azure.com:443/{1}/{2}", this.AccountName, this.Database, this.Collection);
            metadata.SymmetricKey = this.SymmetricKey;
            metadata.Expires = this.Expires;
            metadata.DeviceId = this.Id;
        }
    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to Azure Blob Storage.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "BlobStorageSubscription")]
    public class AddBlobStorageSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The unique identifier associated with the subscription.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "Name of the storage account.", Mandatory = true)]
        public string StorageAccountName;

        [Parameter(HelpMessage = "Name of the blob storage container.", Mandatory = true)]
        public string ContainerName;

        [Parameter(HelpMessage = "(Optional) Shared Access Key.  If used SymmetricKey parameter must be omitted. ", Mandatory = false)]
        public string SharedAccessKey;

        [Parameter(HelpMessage = "(Optional) Symmetric Key of the storage account.  If used SharedAccessKey parameter must be omitted. ", Mandatory = false)]
        public string SymmetricKey;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;

        protected override void ProcessRecord()
        {
            if (!string.IsNullOrEmpty(this.SharedAccessKey) && !string.IsNullOrEmpty(this.SymmetricKey))
            {
                throw new PSArgumentException("SharedAccessKey and Symmetric cannot be used together.");
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata();

            metadata.NotifyAddress = String.Format("https://{0}.blob.core.windows.net/{1}", this.StorageAccountName, this.ContainerName);

            if(!string.IsNullOrEmpty(this.SharedAccessKey))
            {
                metadata.SymmetricKey = this.SharedAccessKey;
            }
            else
            {
                metadata.SymmetricKey = this.SymmetricKey;
            }

            metadata.Expires = this.Expires;
            metadata.DeviceId = this.Id;

        }

    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to an Azure Storage Queue.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "StorageQueueSubscription")]
    public class AddStorageQueueSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The unique identifier associated with the subscription.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "Name of the storage account.", Mandatory = true)]
        public string StorageAccountName;

        [Parameter(HelpMessage = "Name of the storage queue.", Mandatory = true)]
        public string QueueName;

        [Parameter(HelpMessage = "(Optional) Shared Access Key.  If used SymmetricKey parameter must be omitted. ", Mandatory = false)]
        public string SharedAccessKey;

        [Parameter(HelpMessage = "(Optional) Symmetric Key of the storage account.  If used SharedAccessKey parameter must be omitted. ", Mandatory = false)]
        public string SymmetricKey;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;

        protected override void ProcessRecord()
        {
            if(!string.IsNullOrEmpty(this.SharedAccessKey) && !string.IsNullOrEmpty(this.SymmetricKey))
            {
                throw new PSArgumentException("SharedAccessKey and Symmetric cannot be used together.");
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata();

            metadata.NotifyAddress = String.Format("https://{0}.queue.core.windows.net/{1}", this.StorageAccountName, this.QueueName);

            if (!string.IsNullOrEmpty(this.SharedAccessKey))
            {
                metadata.SymmetricKey = this.SharedAccessKey;
            }
            else
            {
                metadata.SymmetricKey = this.SymmetricKey;
            }

            metadata.Expires = this.Expires;
            metadata.DeviceId = this.Id;
        }

    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to an Event Hub.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "EventHubSubscription")]
    public class AddEventHubSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The unique identifier associated with the subscription.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "Service Bus namespace the Event Hub resides.", Mandatory = true)]
        public string ServiceBusNamespace;

        [Parameter(HelpMessage = "Name of the Event Hub.", Mandatory = true)]
        public string HubName;

        [Parameter(HelpMessage = "Event Hub shared access key name.", Mandatory = true)]
        public string SharedAccessKeyName;

        [Parameter(HelpMessage = "Event Hub shared access key.", Mandatory = true)]
        public string SharedAccessKey;

        [Parameter(HelpMessage = "(Optional) Partition ID used only when associating with a single partition in Event Hub.", Mandatory = false)]
        public int PartitionId;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;


        protected override void ProcessRecord()
        {
            if(this.PartitionId < 0)
            {
                throw new PSArgumentOutOfRangeException("Partition ID less than 0.");
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata();

            if (this.PartitionId == 0)
            {
                metadata.NotifyAddress = String.Format("sb://{0}.servicebus.windows.net?keyname={1}&hub={2}", this.ServiceBusNamespace, this.SharedAccessKeyName, this.HubName);
            }
            else
            {
                metadata.NotifyAddress = String.Format("sb://{0}.servicebus.windows.net?keyname={1}&hub={2}&partitionid={3}", this.ServiceBusNamespace, this.SharedAccessKeyName, this.HubName, this.PartitionId.ToString());
            }

            metadata.SymmetricKey = this.SharedAccessKey;
            metadata.Expires = this.Expires;
            metadata.DeviceId = this.Id;
           
        }

    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to a REST Web service.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "RestServiceSubscription")]
    public class AddRestServiceSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "Unique ID that identifies the REST Service.", Mandatory = true)]
        public string Id;

        [Parameter(HelpMessage = "URL of the REST service.", Mandatory = true)]
        public string Address;

        [Parameter(HelpMessage = "(Optional) Security token type required by the REST service.  If JWT or SWT, then the SymmetricKey parameter is required.", Mandatory = false)]
        public SecurityTokenType? TokenType;

        [Parameter(HelpMessage = "(Optional) Symmetric key required to sign security token sent to REST service.  If JWT or SWT TokenType is defined, then the SymmetricKey parameter is required.", Mandatory = false)]
        public string SymmetricKey;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;
        protected override void ProcessRecord()
        {
            SubscriptionMetadata metadata = new SubscriptionMetadata();
            metadata.NotifyAddress = this.Address;
            metadata.TokenType = this.TokenType;
            metadata.SymmetricKey = this.SymmetricKey;
            metadata.Expires = this.Expires;
            metadata.DeviceId = Id;
        }
    }

    /// <summary>
    /// Adds a durable subscription to a topic that sends events to a Service Bus topic.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "ServiceBusSubscription")]
    public class AddServiceBusSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "The topic URI to add the subscription.", Mandatory = true)]
        public string Topic;

        [Parameter(HelpMessage = "The service bus namespace.", Mandatory = true)]
        public string ServiceBusNamespace;

        [Parameter(HelpMessage = "The service bus topic.", Mandatory = true)]
        public string ServiceBusTopic;

        [Parameter(HelpMessage = "Name of shared access key for the service bus topic.", Mandatory = true)]
        public string SharedAccessKeyName;

        [Parameter(HelpMessage = "Shared access key to access the service bus topic.", Mandatory = true)]
        public string SharedAccessKey;

        [Parameter(HelpMessage = "(Optional) Expiration of the subscription.  If omitted never expires.", Mandatory = false)]
        public DateTime? Expires;

        protected override void ProcessRecord()
        {
            SubscriptionMetadata metadata = new SubscriptionMetadata();

            metadata.NotifyAddress = String.Format("sb://{0}.servicebus.windows.net/{1}?keyname={2}", this.ServiceBusNamespace, this.ServiceBusTopic, this.SharedAccessKeyName);
            metadata.SymmetricKey = this.SharedAccessKey;
            metadata.Expires = this.Expires;
        }
    }

    /// <summary>
    /// Removes a subscription from a topic.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Subscription")]
    public class RemoveSubscription : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public string SubscriptionUri;

        protected override void ProcessRecord()
        {
        }
    }

    /// <summary>
    /// Gets a subscription and returns its metadata.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Subscription")]
    public class GetSubscription : Cmdlet
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
    /// Get a list of all subscription URIs for a topic.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SubscriptionList")]
    public class GetSubscriptionList : Cmdlet
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
