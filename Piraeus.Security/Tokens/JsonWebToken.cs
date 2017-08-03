
namespace Piraeus.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Security.Claims;

    public class JsonWebToken : SecurityToken
    {        

        public JsonWebToken(Uri address, string securityKey, string issuer, IEnumerable<Claim> claims)
        {
            id = Guid.NewGuid().ToString();
            created = DateTime.UtcNow;
            expires = created.AddMinutes(20);

            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor msstd = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Issuer = issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                IssuedAt = created,
                NotBefore = created,
                Audience = address.ToString(),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            
            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
            tokenString = jwtToken.ToString();
        }

        public JsonWebToken(Uri audience, string securityKey, string issuer, IEnumerable<Claim> claims, double lifetimeMinutes)
        {
            id = Guid.NewGuid().ToString();
            created = DateTime.UtcNow;
            expires = created.AddMinutes(lifetimeMinutes);
            

            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor msstd = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Issuer = issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                IssuedAt = created,
                NotBefore = created,
                Audience = audience.ToString(),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
            tokenString = jwtToken.ToString();
        }

        //private JwtSecurityTokenHandler handler;
        private DateTime created;
        private DateTime expires;
        private string tokenString;
        private string id;
        public override string ToString()
        {
            return tokenString;
        }

        public void SetSecurityToken(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", String.Format("Bearer {0}", tokenString));
        }


        public override string Id
        {
            get { return this.id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidFrom
        {
            get { return created; }
        }

        public override DateTime ValidTo
        {
            get { return expires; }
        }
    }
}
