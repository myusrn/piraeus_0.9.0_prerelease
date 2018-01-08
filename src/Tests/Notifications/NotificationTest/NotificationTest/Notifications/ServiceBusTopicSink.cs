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
    public class ServiceBusTopicSink : EventSink
    {
        public ServiceBusTopicSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            Uri uri = new Uri(metadata.NotifyAddress);
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


        public override async Task SendAsync(EventMessage message)
        {
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
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }


    }
}

