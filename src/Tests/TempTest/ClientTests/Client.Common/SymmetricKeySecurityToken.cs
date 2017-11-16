using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common
{
    public class SymmetricKeySecurityToken
    {
        public static string CreateJwt(string key, string issuer, string audience, double lifetimeMinutes, List<Claim> claims)
        {
            JsonWebToken token = new JsonWebToken(new Uri(audience), key, issuer, claims, lifetimeMinutes);
            return token.ToString();
        }
    }
}
