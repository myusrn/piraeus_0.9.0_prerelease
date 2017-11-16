using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class SecurityTokenService
    {
        static SecurityTokenService()
        {
            accountTable = ConfigurationManager.AppSettings["AccountTable"];

        }

        private static string accountTable;
        public static string GetSecurityToken(string key, string tokenType)
        {
            List<AccountEntity> list = TableStorage.Get<AccountEntity>(accountTable, "PartitionKey", "eq", key);
            
            if(list == null || list.Count != 1)
            {
                throw new ArgumentOutOfRangeException("key");
            }

            AccountEntity entity = list[0];
            
            //the authority must be in the namespace of the user
            ClaimsManager manager = new ClaimsManager();
            if(!manager.IdentityHasAuthority(entity.Authority))
            {
                throw new SecurityException("Authority not found in identity.");
            }
          



        }
    }
}
