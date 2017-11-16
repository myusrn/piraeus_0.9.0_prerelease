using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StorageServices
{
    public class AccountEntity : TableEntity
    {
        public AccountEntity()
        {
        }

        public AccountEntity(string key, string authority, bool admin, bool tokenRefresh, TimeSpan? duration, IEnumerable<Claim> claims)
        {
            this.Key = key;
            this.Authority = authority;
            this.IsAdmin = admin;
            this.TokenRefresh = tokenRefresh;
            this.TokenDuration = duration;
            this.Claims = claims;
        }

        public AccountEntity(string key, string authority, bool tokenRefresh, TimeSpan? duration, IEnumerable<Claim> claims)
            : this(key, authority, false, tokenRefresh, duration, claims)
        {
        }

        public string Key
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        public string Authority
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string Id { get; set; }

        public bool IsAdmin { get; set; }

        public bool TokenRefresh { get; set; }

        public TimeSpan TokenDuration { get; set; }

        public IEnumerable<Claim> Claims { get; set; }

    }
}
