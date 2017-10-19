using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Security
{
    public class SymmetricKeyParameters
    {
        public SymmetricKeyParameters(string tokenType, string key, string issuer = null, string audience = null)
        {
            TokenType = tokenType;
            Key = key;
            Issuer = issuer;
            Audience = audience;
        }
        public string Issuer { get; internal set; }

        public string Audience { get; internal set; }

        public string TokenType { get; internal set; }

        public string Key { get; internal set; }
    }
}
