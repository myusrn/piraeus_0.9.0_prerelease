using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageServices
{
    public class AuditEntity : TableEntity
    {
        public AuditEntity()
        {

        }

        public AuditEntity(string actorName, string authority, bool allowed, string resource)
        {
            this.ActorName = actorName;
            this.Authority = authority;
            this.Allowed = allowed;
            this.Resource = resource;
        }

        public string ActorName
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        public string Authority
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }
        
        public bool Allowed { get; set; }
        public string Resource { get; set; }
    }
}
