using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Security
{
    public class AsymmetricKeyParameters
    {
        public AsymmetricKeyParameters(string store, string location, string thumbprint)
        {
            Store = (StoreName)Enum.Parse(typeof(StoreName), store, true);
            Location = (StoreLocation)Enum.Parse(typeof(StoreLocation), location, true);
            Thumbprint = thumbprint;
        }

        public StoreName Store { get; internal set; }

        public StoreLocation Location { get; internal set; }

        public string Thumbprint { get; internal set; }
    }
}
