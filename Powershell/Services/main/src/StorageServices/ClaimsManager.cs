using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StorageServices
{
    public class ClaimsManager
    {
        public ClaimsManager()
        {
            authorityClaimType = ConfigurationManager.AppSettings["AuthorityClaimType"];
        }

        private string authorityClaimType;

        public bool IdentityHasAuthority(string authority)
        {
            ClaimsPrincipal principal = Thread.CurrentPrincipal as ClaimsPrincipal;

            ClaimsIdentity identity = principal.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims.Where(x => x.Type == authorityClaimType);

            IEnumerable<Claim> claim = claims.Where(c => c.Value == authority);

            return claim != null;
        }




    }
}
