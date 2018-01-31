using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Storage;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
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
            auditor = new Auditor();
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            container = nvc["container"];

            if (String.IsNullOrEmpty(container))
            {
                container = "$Root";
            }

            string btype = nvc["blobtype"];            

            if(String.IsNullOrEmpty(btype))
            {
                blobType = "block";
            }
            else
            {
                blobType = btype.ToLowerInvariant();
            }

            if (blobType != "block" &&
                blobType != "page" &&
                blobType != "append")
            {
                Trace.TraceWarning("Subscription {0} blob storage sink has invalid Blob Type of {1}", metadata.SubscriptionUriString, blobType);
                return;
            }

            Uri sasUri = null;
            Uri.TryCreate(metadata.SymmetricKey, UriKind.Absolute, out sasUri);

            if (sasUri == null)
            {
                storage = BlobStorage.New(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", uri.Authority.Split(new char[] { '.' })[0], metadata.SymmetricKey),10000,1000);
            }
            else
            {
                string connectionString = String.Format("BlobEndpoint={0};SharedAccessSignature={1}", container != "$Root" ? uri.ToString().Replace(uri.LocalPath, "") : uri.ToString(), metadata.SymmetricKey);
                storage = BlobStorage.New(connectionString,1000,5120000);
            }
        }

        public override async Task SendAsync(EventMessage message)
        {
            if(storage == null)
            {
                Trace.TraceWarning("Subscription {0} did not initialize storage for blob storage sink.  Check logs to determine reason.", metadata.SubscriptionUriString);
                return;
            }

            AuditRecord record = null;
            byte[] payload = null;
           
            try
            {
                payload = GetPayload(message);
                if(payload == null)
                {
                    Trace.TraceWarning("Subscription {0} could not write to blob storage sink because payload was either null or unknown protocol type.");
                    return;
                }

                string filename = GetBlobName(message.ContentType);

                if (blobType == "block")
                {

                    await storage.WriteBlockBlobAsync(container, filename, payload, message.ContentType);
                }
                else if (blobType == "page")
                {
                    await storage.WritePageBlobAsync(container, filename, payload, message.ContentType);
                }
                else
                {
                    await storage.WriteAppendBlobAsync(container, GetAppendFilename(message.ContentType), payload, message.ContentType);
                }

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, false, DateTime.UtcNow, ex.Message);
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

       
        private byte[] GetPayload(EventMessage message)
        {
            switch(message.Protocol)
            {
                case ProtocolType.COAP:
                    CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
                    return coap.Payload;
                case ProtocolType.MQTT:
                    MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
                    return mqtt.Payload;
                case ProtocolType.REST:
                    return message.Message;
                case ProtocolType.WSN:
                    return message.Message;
                default:
                    return null;
            }
        }

        private void Audit(AuditRecord record)
        {
            if (auditor.CanAudit)
            {
                Task task = auditor.WriteAuditRecordAsync(record);
                Task.WhenAll(task);
            }
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
