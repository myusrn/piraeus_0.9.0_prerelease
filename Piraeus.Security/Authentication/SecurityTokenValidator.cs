
namespace Piraeus.Security
{
    //using Piraeus.Configuration;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Microsoft.IdentityModel.Tokens;
    using Piraeus.Security.Tokens;

    public static class SecurityTokenValidator
    {        
        public static bool Validate(string tokenString, SecurityTokenType tokenType, string audience, string issuer, string securityKey)
        {
            if(tokenType == SecurityTokenType.NONE)
            {
                return false;
            }

            if(tokenType == SecurityTokenType.JWT)
            {
                return ValidateJwt(tokenString, audience, issuer, securityKey);
            }
            else if(tokenType == SecurityTokenType.SWT)
            {
                return ValidateSwt(tokenString);
            }
            else
            {
                byte[] certBytes = Convert.FromBase64String(tokenString);
                X509Certificate2 cert = new X509Certificate2(certBytes);
                return ValidateCertificate(cert);
            }
        }

        private static bool ValidateJwt(string tokenString, string audience, string issuer, string signingKey)
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

                Thread.CurrentPrincipal = tokenHandler.ValidateToken(tokenString, validationParameters, out stoken);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
                //bool result = false;
                ////get the issuer, audience, and key from configuration
                //SymmetricKeyTokenInfo info = PiraeusConfigurationManager.GetSigningTokenInfo("JWT");
                //string signingKey = info.SigningKey;
                //string issuer = info.Issuer;
                //string audience = info.Audience;
                //try
                //{
                //    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                //    TokenValidationParameters tvp = new TokenValidationParameters();

                //    TokenValidationParameters validationParameters = new TokenValidationParameters()
                //    {
                //        IssuerSigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(signingKey)),
                //        ValidIssuer = issuer,
                //        ValidAudience = audience
                //    };

                //    SecurityToken securityToken = null;

                //    Thread.CurrentPrincipal = tokenHandler.ValidateToken(tokenString, validationParameters, out securityToken);
                //    result = true;
                //}
                //catch (SecurityTokenValidationException e)
                //{
                //    Trace.TraceWarning("JWT security token validation exception.");
                //    Trace.TraceError(e.Message);
                //}
                //catch (Exception ex)
                //{
                //    Trace.TraceWarning("JWT security token authentication exception.");
                //    Trace.TraceError(ex.Message);
                //}

                //return result;
            }


        private static bool ValidateSwt(string tokenString)
        {
            bool result = false;

            //get key from cofiguration
            string key = null;

            try
            {
                SimpleWebToken token = SimpleWebToken.FromString(tokenString);
                result = token.SignVerify(Convert.FromBase64String(key));
                
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
