
namespace SkunkLab.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using Microsoft.IdentityModel.Tokens;

    public class JsonWebToken : System.IdentityModel.Tokens.SecurityToken
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
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
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
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(securityKey)), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            
            JwtSecurityToken jwtToken = jwt.CreateJwtSecurityToken(msstd);
            tokenString = jwt.WriteToken(jwtToken);
        }

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

        

        public override DateTime ValidFrom
        {
            get { return created; }
        }

        public override DateTime ValidTo
        {
            get { return expires; }
        }

        public override ReadOnlyCollection<System.IdentityModel.Tokens.SecurityKey> SecurityKeys => throw new NotImplementedException();

        public static void Authenticate(string token, string issuer, string audience, string signingKey)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true
                };

                Microsoft.IdentityModel.Tokens.SecurityToken stoken = null;

                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, out stoken);

            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException e)
            {
                Trace.TraceWarning("JWT validation has security token exception.");
                Trace.TraceError(e.Message);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Exception in JWT validation.");
                Trace.TraceError(ex.Message);
            }
        }
        
    }
}
