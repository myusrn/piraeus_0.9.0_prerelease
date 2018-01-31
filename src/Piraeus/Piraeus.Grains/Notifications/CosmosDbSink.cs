using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class CosmosDBSink : EventSink
    {
        private Uri uri;
        private DocumentClient client;
        private Uri documentDBUri;
        private string databaseId;
        private string symmetricKey;
        private string collectionId;
        private Database database;
        private DocumentCollection collection;
        private Auditor auditor;

        public CosmosDBSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            auditor = new Auditor();
            uri = new Uri(metadata.NotifyAddress);
            string docDBUri = String.Format("https://{0}", uri.Authority);
            documentDBUri = new Uri(String.Format("https://{0}", uri.Authority));

            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            databaseId = nvc["database"];
            collectionId = nvc["collection"];

            symmetricKey = metadata.SymmetricKey;
            client = new DocumentClient(documentDBUri, symmetricKey);
        }

        


        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
            byte[] payload = null;

            try
            {
                payload = GetPayload(message);
                if (payload == null)
                {
                    Trace.TraceWarning("Subscription {0} could not write to CosmosDB sink because payload was either null or unknown protocol type.");
                    return;
                }

                if (client == null)
                {
                    client = new DocumentClient(documentDBUri, symmetricKey);
                }

                if (database == null)
                {
                    database = await GetDatabaseAsync();
                }

                if (collection == null)
                {
                    collection = await GetCollectionAsync(database.SelfLink, collectionId);
                }

                using (MemoryStream stream = new MemoryStream(payload))
                {
                    stream.Position = 0;
                    if (message.ContentType.Contains("json"))
                    {


                        await client.CreateDocumentAsync(collection.SelfLink, Microsoft.Azure.Documents.Resource.LoadFrom<Document>(stream));
                    }
                    else
                    {
                        dynamic documentWithAttachment = new
                        {
                            Id = Guid.NewGuid().ToString(),
                            Timestamp = DateTime.UtcNow
                        };

                        Document doc = await client.CreateDocumentAsync(collection.SelfLink, documentWithAttachment);
                        string slug = GetSlug(documentWithAttachment.Id, message.ContentType);
                        await client.CreateAttachmentAsync(doc.AttachmentsLink, stream, new MediaOptions { ContentType = message.ContentType, Slug = slug });
                    }
                }

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "CosmosDB", "CosmoDB", payload.Length, true, DateTime.UtcNow);

                
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "CosmosDB", "CosmosDB", payload.Length, false, DateTime.UtcNow, ex.Message);
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
            switch (message.Protocol)
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

        private string GetSlug(string id, string contentType)
        {
            if (contentType.Contains("text"))
            {
                return id + ".txt";
            }
            else if (contentType.Contains("xml"))
            {
                return id + ".xml";
            }
            else
            {
                return id;
            }
        }


        private async Task<Database> GetDatabaseAsync()
        {
            try
            {
                List<Database> dbs = await ListDatabasesAsync();

                foreach (Database db in dbs)
                {
                    if (db.Id == databaseId)
                    {
                        return db;
                    }
                }

                return await client.CreateDatabaseAsync(new Database { Id = databaseId });
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Cannot return or create Doc DB database.");
                Trace.TraceError(ex.Message);
                throw;
            }
        }

        private async Task<DocumentCollection> GetCollectionAsync(string dbLink, string id)
        {
            List<DocumentCollection> collections = await ReadCollectionsFeedAsync(dbLink);
            if (collections != null)
            {
                foreach (DocumentCollection collection in collections)
                {
                    if (collection.Id == id)
                    {
                        return collection;
                    }
                }
            }

            return await client.CreateDocumentCollectionAsync(dbLink, new DocumentCollection() { Id = id });
        }

        private async Task<List<DocumentCollection>> ReadCollectionsFeedAsync(string databaseSelfLink)
        {
            Exception exception = null;
            string continuation = null;
            List<DocumentCollection> collections = new List<DocumentCollection>();

            try
            {
                do
                {
                    FeedOptions options = new FeedOptions
                    {
                        RequestContinuation = continuation,
                        MaxItemCount = 50
                    };

                    FeedResponse<DocumentCollection> response = (FeedResponse<DocumentCollection>)await client.ReadDocumentCollectionFeedAsync(databaseSelfLink, options);

                    foreach (DocumentCollection col in response)
                    {
                        collections.Add(col);
                    }

                    continuation = response.ResponseContinuation;

                } while (!String.IsNullOrEmpty(continuation));

                return collections;
            }
            catch (DocumentClientException dce)
            {
                exception = dce;
            }
            catch (AggregateException ae)
            {
                exception = ae;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                Trace.TraceWarning("Exception locating Document DB collection.");
                Trace.TraceError(exception.Message);
                throw exception;
            }
            else
            {
                return null;
            }

        }

        private async Task<List<Database>> ListDatabasesAsync()
        {
            string continuation = null;
            List<Database> databases = new List<Database>();

            do
            {
                FeedOptions options = new FeedOptions
                {
                    RequestContinuation = continuation,
                    MaxItemCount = 50
                };

                FeedResponse<Database> response = await client.ReadDatabaseFeedAsync(options);
                foreach (Database db in response)
                {
                    databases.Add(db);
                }

                continuation = response.ResponseContinuation;
            }
            while (!String.IsNullOrEmpty(continuation));

            return databases;
        }
       
    }
}
