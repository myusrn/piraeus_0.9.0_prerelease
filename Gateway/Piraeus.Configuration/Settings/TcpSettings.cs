using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    public class TcpSettings
    {
        
        public TcpSettings(bool useLengthPrefix = true, bool authenticate = false, X509Certificate2 certificate = null, string pskIdentity = null, byte[] pskKey = null)
        {
            UseLengthPrefix = useLengthPrefix;
            Authenticate = authenticate;
            Certificate = certificate;
            PskIdentity = pskIdentity;
            PskKey = PskKey;
        }
        public bool UseLengthPrefix { get; set; }
        public bool Authenticate { get; set; }
        public X509Certificate2 Certificate { get; set; }
        public string PskIdentity { get; set; }
        public byte[] PskKey { get; set; }
    }
}
