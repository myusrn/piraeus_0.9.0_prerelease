using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class StorageManager
    {
        static StorageManager()
        {
            accountTable = ConfigurationManager.AppSettings["AccountTable"];
            auditTable = ConfigurationManager.AppSettings["AuditTable"];
        }

        private static string accountTable;
        private static string auditTable;

        #region Account
        public void AddAccount(string key, string authority, bool admin, IEnumerable<Claim> claims)
        {
            AccountEntity entity = new AccountEntity(key, authority, admin, claims);
            TableStorage.Insert(accountTable, entity);
        }

        public async Task AddAccountAsync(string key, string authority, bool admin, IEnumerable<Claim> claims)
        {
            AccountEntity entity = new AccountEntity(key, authority, admin, claims);
            await TableStorage.InsertAsync(accountTable, entity);
        }

        public void DeleteAccount(string key)
        {
            AccountEntity entity = TableStorage.Get<AccountEntity>(accountTable, "PartitionKey", "eq", key);
            if(entity != null)
            {
                TableStorage.Delete(accountTable, entity);
            }
            else
            {
                throw new ArgumentOutOfRangeException("key");
            }
        }

        public async Task DeleteAccountAsync(string key)
        {
            AccountEntity entity = TableStorage.Get<AccountEntity>(accountTable, "PartitionKey", "eq", key);
            if (entity != null)
            {
                await TableStorage.DeleteAsync(accountTable, entity);
            }
            else
            {
                throw new ArgumentOutOfRangeException("key");
            }
        }

        #endregion

        #region Audit

        public void WriteAudit(string actorName, string authority, bool allowed, string resource)
        {
            AuditEntity entity = new AuditEntity(actorName, authority, allowed, resource);
            TableStorage.Insert(auditTable, entity);
        }

        public async Table WriteAuditAsync(string actorName, string authority, bool allowed, string resource)
        {
            AuditEntity entity = new AuditEntity(actorName, authority, allowed, resource);
            await TableStorage.InsertAsync(auditTable, entity);
        }

        #endregion


    }
}
