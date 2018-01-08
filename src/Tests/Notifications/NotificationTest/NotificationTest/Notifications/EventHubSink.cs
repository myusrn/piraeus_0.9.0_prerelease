using Microsoft.ServiceBus.Messaging;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;

namespace NotificationTest.Notifications
{
    public class EventHubSink : EventSink
    {
        private EventHubSender sender;
        private EventHubClient client;

        public EventHubSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            Uri uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            keyName = nvc["keyname"];
            partitionId = nvc["partitionid"];
            hubName = nvc["hub"];
            connectionString = String.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, metadata.SymmetricKey);
        }


        private string keyName;
        private string partitionId;
        private string hubName;
        private string connectionString;

        public override async Task SendAsync(EventMessage message)
        {
            EventData data = new EventData(message.Message);
            data.Properties.Add("Content-Type", message.ContentType);

            if (client == null)
            {
                client = EventHubClient.CreateFromConnectionString(connectionString, hubName);
                Trace.TraceInformation("Event Hub client created.");
            }

            if (!string.IsNullOrEmpty(partitionId) && sender == null)
            {
                sender = client.CreatePartitionedSender(partitionId);
                Trace.TraceInformation("Event Hub sender created with partition ID.");
            }

            if (string.IsNullOrEmpty(partitionId))
            {
                try
                {
                    await client.SendAsync(data);
                    Trace.TraceInformation("Event Hub client sent data.");
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Event hub client failed to send data.");
                    Trace.TraceError(ex.Message);
                }
            }
            else
            {
                try
                {
                    await sender.SendAsync(data);
                    Trace.TraceInformation("Event Hub sender sent data.");
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Event hub sender failed to send data.");
                    Trace.TraceError(ex.Message);
                }
            }
        }
    }
}
