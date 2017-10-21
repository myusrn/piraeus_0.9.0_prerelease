using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    public class ServiceSecurity
    {

        public ServiceSecurity(X509Certificate2 certificate = null)
        {
            Certificate = certificate;
        }

        public X509Certificate2 Certificate { get; set; }
    }
}
