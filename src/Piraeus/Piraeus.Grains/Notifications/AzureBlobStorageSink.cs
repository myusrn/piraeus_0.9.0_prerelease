using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Storage;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class AzureBlobStorageSink : EventSink
    {
        private BlobStorage storage;
        private string container;
        private string blobType;
        private string appendFilename;
        private Auditor auditor;
        private Uri uri;
        
        public AzureBlobStorageSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            container = nvc["container"];

            if (String.IsNullOrEmpty(container))
            {
                container = "$Root";
            }

            blobType = nvc["blobtype"];

            if (String.IsNullOrEmpty(blobType) || (blobType.ToLowerInvariant() != "block" &&
                blobType.ToLowerInvariant() != "page" &&
                blobType.ToLowerInvariant() != "append"))
            {
                blobType = "block";
            }

            Uri sasUri = null;
            Uri.TryCreate(metadata.SymmetricKey, UriKind.Absolute, out sasUri);

            if (sasUri == null)
            {
                storage = BlobStorage.New(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", uri.Authority.Split(new char[] { '.' })[0], metadata.SymmetricKey));
            }
            else
            {
                string connectionString = String.Format("BlobEndpoint={0};SharedAccessSignature={1}", container != "$Root" ? uri.ToString().Replace(uri.LocalPath, "") : uri.ToString(), metadata.SymmetricKey);
                storage = BlobStorage.New(connectionString);
            }

            
        }

        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
           
            try
            {                
                string filename = GetBlobName(message.ContentType);

                if (blobType == "block")
                {

                    await storage.WriteBlockBlobAsync(container, filename, message.Message, message.ContentType);
                }
                else if (blobType == "page")
                {
                    await storage.WritePageBlobAsync(container, filename, message.Message, message.ContentType);
                }
                else
                {
                    await storage.WriteAppendBlobAsync(container, GetAppendFilename(message.ContentType), message.Message, message.ContentType);
                }

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", message.Message.Length, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", message.Message.Length, false, DateTime.UtcNow, ex.Message);
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
            if(auditor == null)
            {
                auditor = new Auditor();
            }

            Task task = auditor.WriteAuditRecordAsync(record);
            Task.WhenAll(task);
        }


        private string GetAppendFilename(string contentType)
        {
            if (appendFilename == null)
            {
                appendFilename = GetBlobName(contentType);
            }

            return appendFilename;
        }

        private string GetBlobName(string contentType)
        {
            string suffix = null;
            if (contentType.Contains("text"))
            {
                suffix = "txt";
            }
            else if (contentType.Contains("json"))
            {
                suffix = "json";
            }
            else if (contentType.Contains("xml"))
            {
                suffix = "xml";
            }

            string filename = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss-fffff");
            return suffix == null ? filename : String.Format("{0}.{1}", filename, suffix);
        }
    }
}
