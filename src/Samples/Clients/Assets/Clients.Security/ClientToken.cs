using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Clients.Security
{
    public class ClientToken
    {

        public static string CreateJwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            SkunkLab.Security.Tokens.JsonWebToken jwt = new SkunkLab.Security.Tokens.JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, lifetimeMinutes);
            return jwt.ToString();
        }

        public static string CreateSwt(string audience, string issuer, List<Claim> claims, string symmetricKey, double lifetimeMinutes)
        {
            DateTime expiry = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
            SkunkLab.Security.Tokens.SimpleWebToken swt = new SkunkLab.Security.Tokens.SimpleWebToken(issuer, audience, expiry, claims);
            swt.Sign(Convert.FromBase64String(symmetricKey));
            return swt.ToString();
        }

        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            return null;
        }

        public string Acquire(string endpoint, string key)
        {
            return null;
        }
    }
}
