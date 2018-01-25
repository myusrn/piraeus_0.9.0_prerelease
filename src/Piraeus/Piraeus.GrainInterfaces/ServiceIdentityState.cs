using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class ServiceIdentityState
    {
        public X509Certificate2 Certificate { get; set; }

        public List<Claim> Claims { get; set; }
    }
}
