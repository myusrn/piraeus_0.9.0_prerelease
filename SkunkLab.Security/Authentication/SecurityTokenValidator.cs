
namespace SkunkLab.Security.Authentication
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Microsoft.IdentityModel.Tokens;
    using SkunkLab.Security.Tokens;

    public static class SecurityTokenValidator
    {        
        public static bool Validate(string tokenString, SecurityTokenType tokenType, string securityKey, string issuer = null, string audience = null)
        {
            if(tokenType == SecurityTokenType.NONE)
            {
                return false;
            }

            if(tokenType == SecurityTokenType.JWT)
            {
                return ValidateJwt(tokenString, securityKey, issuer, audience);
            }
            else if(tokenType == SecurityTokenType.SWT)
            {
                return ValidateSwt(tokenString, issuer, audience);
            }
            else
            {
                byte[] certBytes = Convert.FromBase64String(tokenString);
                X509Certificate2 cert = new X509Certificate2(certBytes);
                return ValidateCertificate(cert);
            }
        }

        private static bool ValidateJwt(string tokenString, string signingKey, string issuer = null, string audience = null)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidateAudience = audience != null,
                    ValidateIssuer = issuer != null,
                    ValidateIssuerSigningKey = true
                };

                Microsoft.IdentityModel.Tokens.SecurityToken stoken = null;

                Thread.CurrentPrincipal = tokenHandler.ValidateToken(tokenString, validationParameters, out stoken);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("JWT validation exception {0}", ex.Message);
                return false;
            }

        }


        private static bool ValidateSwt(string tokenString, string issuer = null, string audience = null)
        {
            bool result = false;

            //get key from cofiguration
            string key = null;

            try
            {
                SimpleWebToken token = SimpleWebToken.FromString(tokenString);
                if(!token.SignVerify(Convert.FromBase64String(key)))
                {
                    throw new System.IdentityModel.Tokens.SecurityTokenException("SWT cannot be verified.");
                }

                if(audience != null && token.Audience.ToLower(CultureInfo.InvariantCulture) != audience.ToLower(CultureInfo.InvariantCulture))
                {
                    throw new System.IdentityModel.Tokens.SecurityTokenException("SWT audience mismatch.");
                }

                if(issuer != null && token.Issuer.ToLower(CultureInfo.InvariantCulture) != issuer.ToLower(CultureInfo.InvariantCulture))
                {
                    throw new System.IdentityModel.Tokens.SecurityTokenException("SWT issuer mismatch.");
                }

                if(token.ExpiresOn < DateTime.UtcNow)
                {
                    throw new System.IdentityModel.Tokens.SecurityTokenException("SWT token has expired.");
                }

                ClaimsPrincipal principal = new ClaimsPrincipal(token.Identity);
                Thread.CurrentPrincipal = principal;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("SWT validation exception.");
                Trace.TraceError(ex.Message);
            }

            return result;
        }

        private static bool ValidateCertificate(X509Certificate2 cert)
        {
            try
            {
                X509SecurityTokenHandler handler = new X509SecurityTokenHandler(X509CertificateValidator.PeerOrChainTrust);
                X509SecurityToken token = new X509SecurityToken(cert);
                ReadOnlyCollection<ClaimsIdentity> col = handler.ValidateToken(token);
                ClaimsPrincipal principal = new ClaimsPrincipal(col[0]);
                Thread.CurrentPrincipal = principal;
                return true;
            }
            catch(Exception ex)
            {
                Trace.TraceError(String.Format("X509 validation exception '{0}'", ex.Message));
                return false;
            }
        }
    }
}
