using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using SkunkLab.Security.Tokens;
using System.Web;

namespace GrainTests
{
    public class Identity
    {
        public string GetIdentity(string key, string issuer, string audience, List<Claim> claims)
        {
            JsonWebToken jwt = new JsonWebToken(new Uri(audience), key, issuer, claims);
            string tokenString = jwt.ToString();
            SkunkLab.Security.Authentication.SecurityTokenValidator.Validate(tokenString, SecurityTokenType.JWT, key, issuer, audience);
            return tokenString;            
        }


        //public void D()
        //{
        //    Uri address = new Uri("http://www.skunklab.io/");
        //    string issuer = "http://www.skunklab.io/";
        //    string key = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        //    List<Claim> list = new List<Claim>()
        //    {
        //        new Claim("http://www.skunklab.io/piraeus/role", "sub"),
        //        new Claim("http://www.example.org/name", "matts")
        //    };

        //    JsonWebToken token = new JsonWebToken(address, key, issuer, list);
        //    return token.ToString();
        //}
    }
}
