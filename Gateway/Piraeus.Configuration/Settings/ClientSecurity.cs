namespace Piraeus.Configuration.Settings
{
    public class ClientSecurity
    {
        public ClientSecurity()
        {

        }

        public ClientSecurity(string tokenType, string symmetricKey, string issuer = null, string audience = null)
        {
            TokenType = tokenType;
            SymmetricKey = symmetricKey;
            Issuer = issuer;
            Audience = audience;
        }
        public string TokenType { get; set; }

        public string SymmetricKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }
    }
}
