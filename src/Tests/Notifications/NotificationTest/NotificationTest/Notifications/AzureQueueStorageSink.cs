using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Storage;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;

namespace NotificationTest.Notifications
{
    public class AzureQueueStorageSink : EventSink
    {
        private QueueStorage storage;
        private string queue;
        private TimeSpan? ttl;

        public AzureQueueStorageSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            Uri addressUri = new Uri(metadata.NotifyAddress);

            Uri uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            queue = nvc["queue"];

            string ttlString = nvc["ttl"];
            if(!String.IsNullOrEmpty(ttlString))
            {
                ttl = TimeSpan.Parse(ttlString);
            }


            Uri sasUri = null;
            Uri.TryCreate(metadata.SymmetricKey, UriKind.Absolute, out sasUri);

            if (sasUri == null)
            {
                storage = QueueStorage.New(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", addressUri.Authority.Split(new char[] { '.' })[0], metadata.SymmetricKey));
            }
            else
            {
                string connectionString = String.Format("BlobEndpoint={0};SharedAccessSignature={1}", queue, metadata.SymmetricKey);
                storage = QueueStorage.New(connectionString);
            }
        }

        public override async Task SendAsync(EventMessage message)
        {
            await storage.EnqueueAsync(queue, message.Message, ttl);
        }

    }
}
