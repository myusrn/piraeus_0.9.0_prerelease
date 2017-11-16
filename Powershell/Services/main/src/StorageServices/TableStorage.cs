using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class TableStorage
    {
        static TableStorage()
        {
            storageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];
            storageKey = ConfigurationManager.AppSettings["StorageKey"];
            nameClaimType = ConfigurationManager.AppSettings["NameClaimType"];
            roleClaimType = ConfigurationManager.AppSettings["RoleClaimType"];
            authorityClaimType = ConfigurationManager.AppSettings["AuthorityClaimType"];
            auditTableName = ConfigurationManager.AppSettings["AuditLogTableName"];
            policyReferenceTableName = ConfigurationManager.AppSettings["PolicyReferenceTableName"];
            StorageCredentials creds = new StorageCredentials(storageAccountName, storageKey);
            client = new CloudTableClient(new Uri(String.Format("https://{0}.table.core.windows.net/", storageAccountName)), creds);
        }

        private static CloudTableClient client;
        private static string nameClaimType;
        private static string roleClaimType;
        private static string authorityClaimType;
        private static string auditTableName;
        private static string policyReferenceTableName;
        private static string storageAccountName; 
        private static string storageKey;

        public static async Task InsertAsync(string tableName, TableEntity entity)
        {
            await Retry.ExecuteAsync(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();

                TableOperation insertOperation = TableOperation.Insert(entity);
                await table.ExecuteAsync(insertOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static void Insert(string tableName, TableEntity entity)
        {
            Retry.Execute(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();

                TableOperation insertOperation = TableOperation.Insert(entity);
                await table.ExecuteAsync(insertOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static async Task UpdateAsync(string tableName, TableEntity entity)
        {
            await Retry.ExecuteAsync(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                TableOperation updateOperation = TableOperation.InsertOrReplace(entity);
                await table.ExecuteAsync(updateOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static void Update(string tableName, TableEntity entity)
        {
            Retry.Execute(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                TableOperation updateOperation = TableOperation.InsertOrReplace(entity);
                await table.ExecuteAsync(updateOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static async Task DeleteAsync(string tableName, TableEntity entity)
        {            
            await Retry.ExecuteAsync(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await table.ExecuteAsync(deleteOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static void Delete(string tableName, TableEntity entity)
        {
            Retry.Execute(async () =>
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await table.ExecuteAsync(deleteOperation);
            }, TimeSpan.FromSeconds(2), 3);
        }

        public static async Task<List<TEntity>> GetAsync<TEntity>(string tableName, string fieldName, string comparer, string value) where TEntity : TableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<TEntity>().Where(TableQuery.GenerateFilterCondition(fieldName, comparer, value));
            TableQuerySegment<TEntity> segment = table.ExecuteQuerySegmented<TEntity>(query, new TableContinuationToken());
            
            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }

        }

        public static List<TEntity> Get<TEntity>(string tableName, string fieldName, string comparer, string value) where TEntity : TableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            table.CreateIfNotExists();

            var query = new TableQuery<TEntity>().Where(TableQuery.GenerateFilterCondition(fieldName, comparer, value));
            TableQuerySegment<TEntity> segment = table.ExecuteQuerySegmented<TEntity>(query, new TableContinuationToken());
            
            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }
                
        public static async Task AuditAsync()
        {
            ClaimsPrincipal principal = Thread.CurrentPrincipal as ClaimsPrincipal;
            string name = "unknown";
            string role = "unknown";

            if (principal != null)
            {

                if (principal.HasClaim(c => c.Type.ToLower(CultureInfo.InvariantCulture) == nameClaimType))
                {
                    name = principal.FindFirst(nameClaimType).Value;
                }

                if (principal.HasClaim(c => c.Type.ToLower(CultureInfo.InvariantCulture) == roleClaimType))
                {
                    role = principal.FindFirst(roleClaimType).Value;
                }
            }

            //await Retry.ExecuteAsync(async () =>
            //{
            //    CloudTable table = client.GetTableReference(auditTableName);
            //    await table.CreateIfNotExistsAsync();

            //    AuditEntity entity = new AuditEntity(name, role);

            //    TableOperation insertOperation = TableOperation.Insert(entity);
            //    await table.ExecuteAsync(insertOperation);
            //}, TimeSpan.FromSeconds(2), 3);

        }

    }
}
