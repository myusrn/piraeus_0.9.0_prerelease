using Microsoft.ServiceBus.Messaging;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class EventHubSink : EventSink
    {
        private EventHubSender sender;
        private EventHubClient client;
        private Uri uri;
        private Auditor auditor;
        private string keyName;
        private string partitionId;
        private string hubName;
        private string connectionString;

        public EventHubSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            keyName = nvc["keyname"];
            partitionId = nvc["partitionid"];
            hubName = nvc["hub"];
            connectionString = String.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, metadata.SymmetricKey);
            client = EventHubClient.CreateFromConnectionString(connectionString, hubName);
            if(!String.IsNullOrEmpty(partitionId))
            {
                sender = client.CreatePartitionedSender(partitionId);
            }
        }


        

        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;

            try
            {
                EventData data = new EventData(message.Message);
                data.Properties.Add("Content-Type", message.ContentType);                

                if (String.IsNullOrEmpty(partitionId))
                {
                    await client.SendAsync(data);
                }
                else
                {
                    await sender.SendAsync(data);
                }

                record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}", uri.Authority, hubName), "EventHub", "EventHub", message.Message.Length, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}", uri.Authority, hubName), "EventHub", "EventHub", message.Message.Length, false, DateTime.UtcNow, ex.Message);
                throw;
            }
            finally
            {
                if(message.Audit)
                {
                    Audit(record);
                }
            }
        }

        private void Audit(AuditRecord record)
        {
            if (auditor == null)
            {
                auditor = new Auditor();
            }

            Task task = auditor.WriteAuditRecordAsync(record);
            Task.WhenAll(task);
        }
    }
}
