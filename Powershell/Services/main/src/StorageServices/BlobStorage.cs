using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class BlobStorage
    {
        static BlobStorage()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            client = storageAccount.CreateCloudBlobClient();
        }

        private static CloudStorageAccount storageAccount;
        private static CloudBlobClient client;

        public static async Task CreateBlobAsync(string container, string filename, byte[] blob, string contentType)
        {
            CloudBlobContainer blobContainer = client.GetContainerReference(container);            
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);           
            blockBlob.Properties.ContentType = contentType;
            await blockBlob.UploadFromByteArrayAsync(blob, 0, blob.Length);
        }

        public static void CreateBlob(string container, string filename, byte[] blob, string contentType)
        {
            CloudBlobContainer blobContainer = client.GetContainerReference(container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            blockBlob.Properties.ContentType = contentType;
            blockBlob.UploadFromByteArray(blob, 0, blob.Length);
        }

        public static async Task<byte[]> GetBlobAsync(string container, string filename)
        {
            byte[] buffer = null;
            CloudBlobContainer blobContainer = client.GetContainerReference(container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            using(MemoryStream stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                buffer = stream.ToArray();
            }

            return buffer;
        }

        public static byte[] GetBlob(string container, string filename)
        {
            byte[] buffer = null;
            CloudBlobContainer blobContainer = client.GetContainerReference(container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            using (MemoryStream stream = new MemoryStream())
            {
                blockBlob.DownloadToStream(stream);
                stream.Position = 0;
                buffer = stream.ToArray();
            }

            return buffer;
        }


    }
}
