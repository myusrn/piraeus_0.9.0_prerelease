using System.Collections.Generic;
using System.Security.Claims;

namespace Piraeus.Configuration.Settings
{
    public class ServiceIdentity
    {
        public ServiceIdentity()
        {

        }

        public ServiceIdentity(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        public ServiceIdentity(List<KeyValuePair<string,string>> claims)
        {
            List<Claim> claimList = new List<Claim>();
            foreach(var claim in claims)
            {
                claimList.Add(new Claim(claim.Key, claim.Value));
            }

            Claims = claimList;
        }

        public IEnumerable<Claim> Claims { get; set; }




    }
}
