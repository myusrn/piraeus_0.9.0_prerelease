using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Storage;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class AzureQueueStorageSink : EventSink
    {
        private QueueStorage storage;
        private string queue;
        private TimeSpan? ttl;
        private Uri uri;
        private Auditor auditor;

        public AzureQueueStorageSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            queue = nvc["queue"];

            string ttlString = nvc["ttl"];
            if (!String.IsNullOrEmpty(ttlString))
            {
                ttl = TimeSpan.Parse(ttlString);
            }


            Uri sasUri = null;
            Uri.TryCreate(metadata.SymmetricKey, UriKind.Absolute, out sasUri);

            if (sasUri == null)
            {
                storage = QueueStorage.New(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", uri.Authority.Split(new char[] { '.' })[0], metadata.SymmetricKey));
            }
            else
            {
                string connectionString = String.Format("BlobEndpoint={0};SharedAccessSignature={1}", queue, metadata.SymmetricKey);
                storage = QueueStorage.New(connectionString);
            }
        }

        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;

            try
            {
                await storage.EnqueueAsync(queue, message.Message, ttl);

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureQueue", "AzureQueue", message.Message.Length, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureQueue", "AzureQueue", message.Message.Length, false, DateTime.UtcNow, ex.Message);
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
