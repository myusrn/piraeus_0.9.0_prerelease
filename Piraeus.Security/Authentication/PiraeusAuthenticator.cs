using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Security.Authentication
{
    public class PiraeusAuthenticator : IAuthenticator
    {

        public PiraeusAuthenticator()
        {
            container = new Dictionary<SecurityTokenType, SecurityTokenParameters>();
        }

        private Dictionary<SecurityTokenType, SecurityTokenParameters> container;

        public void AddParameters(SecurityTokenType tokenType, SecurityTokenParameters parameters)
        {
            container.Add(tokenType, parameters);
        }

        public void RemoveParameters(SecurityTokenType tokenType)
        {
            container.Remove(tokenType);
        }

        public bool ContainsKey(SecurityTokenType tokenType)
        {
            return ContainsKey(tokenType);
        }

        
        public bool Authenticate(SecurityTokenType type, byte[] token)
        {                           
            if(type == SecurityTokenType.X509)
            {
                return SecurityTokenValidator.Validate(Convert.ToBase64String(token), type, null, null, null);
            }
            else
            {
                return SecurityTokenValidator.Validate(Encoding.UTF8.GetString(token), type, container[type].Audience, container[type].Issuer, container[type].SigningKey);
            }
        }

        public bool Authenticate(SecurityTokenType type, string token)
        {
            if (type == SecurityTokenType.X509)
            {
                return SecurityTokenValidator.Validate(token, type, null, null, null);
            }
            else
            {
                return SecurityTokenValidator.Validate(token, type, container[type].Audience, container[type].Issuer, container[type].SigningKey);
            }
        }
    }
}
