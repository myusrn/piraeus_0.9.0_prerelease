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
    public class ServiceBusTopicSink : EventSink
    {
        public ServiceBusTopicSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            keyName = nvc["keyname"];
            topic = nvc["topic"];
            string symmetricKey = metadata.SymmetricKey;
            connectionString = String.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", uri.Authority, keyName, symmetricKey);
        }

        private string keyName;
        private string topic;
        private string connectionString;
        private TopicClient client;
        private Auditor auditor;
        private Uri uri;


        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;

            try
            {
                if (client == null)
                {
                    client = TopicClient.CreateFromConnectionString(connectionString, topic);
                }

                BrokeredMessage brokerMessage = new BrokeredMessage(Convert.ToBase64String(message.Message));
                brokerMessage.Properties.Add("Content-Type", message.ContentType);
                brokerMessage.MessageId = message.MessageId;
                await client.SendAsync(brokerMessage);
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}",uri.Authority, topic), "ServiceBus", "ServiceBus", message.Message.Length, true, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Service bus failed to send to topic with error {0}",ex.Message);
                record = new AuditRecord(message.MessageId, String.Format("sb://{0}/{1}", uri.Authority, topic), "ServiceBus", "ServiceBus", message.Message.Length, false, DateTime.UtcNow, ex.Message);
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
