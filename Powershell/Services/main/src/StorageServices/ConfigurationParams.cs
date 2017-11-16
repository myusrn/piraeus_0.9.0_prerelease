using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class ConfigurationParams
    {
        static ConfigurationParams()
        {
            AuthorityClaimType = ConfigurationManager.AppSettings["AuthorityClaimType"];
            AccountTable = ConfigurationManager.AppSettings["AccountTable"];
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            storageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];
            storageKey = ConfigurationManager.AppSettings["StorageKey"];
            NameClaimType = ConfigurationManager.AppSettings["NameClaimType"];
            RoleClaimType = ConfigurationManager.AppSettings["RoleClaimType"];
            AuditTable = ConfigurationManager.AppSettings["AuditTable"];
            PolicyReferenceTable = ConfigurationManager.AppSettings["PolicyReferenceTableName"];
            StorageCredentials creds = new StorageCredentials(storageAccountName, storageKey);
            TableClient = new CloudTableClient(new Uri(String.Format("https://{0}.table.core.windows.net/", storageAccountName)), creds);
            BlobClient = storageAccount.CreateCloudBlobClient();

        }

     

        public static string AuthorityClaimType { get; internal set; }

        public static string NameClaimType { get; internal set; }

        public static string RoleClaimType { get; internal set; }

        public static string AuditTable { get; internal set; }

        public static string AccountTable { get; internal set; }

        public static string PolicyReferenceTable { get; internal set; }

        public static CloudBlobClient BlobClient { get; internal set; }

        public static CloudTableClient TableClient { get; internal set; }
       

    }
}
