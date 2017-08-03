using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Security.Authentication
{
    public class SecurityTokenParameters
    {
        public SecurityTokenParameters(string audience, string issuer, string signingKey)
        {
            Uri uri = new Uri(audience.ToLower(CultureInfo.InvariantCulture));
            Audience = uri.ToString();
            Issuer = issuer;
            SigningKey = signingKey;
        }
        public string Audience { get; internal set; }

        public string Issuer { get; internal set; }

        public string SigningKey { get; internal set; }


    }
}
