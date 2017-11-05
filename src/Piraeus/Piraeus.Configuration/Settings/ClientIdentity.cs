using System.Collections.Generic;

namespace Piraeus.Configuration.Settings
{
    public class ClientIdentity
    {
        public ClientIdentity()
        {

        }

        public ClientIdentity(string identityClaimType, List<KeyValuePair<string,string>> indexes = null)
        {
            IdentityClaimType = identityClaimType;
            Indexes = indexes;
        }
        public string IdentityClaimType { get; set; }

        public List<KeyValuePair<string,string>> Indexes { get; set; }
    }
}
