

namespace SkunkLab.Security.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.IdentityModel.Tokens;

    public class X509ValidationHandler : DelegatingHandler
    {
        public X509ValidationHandler()
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;

            try
            {
                X509Certificate2 cert = request.GetClientCertificate();
                X509SecurityTokenHandler handler = new X509SecurityTokenHandler(X509CertificateValidator.PeerOrChainTrust);
                X509SecurityToken token = new X509SecurityToken(cert);
                ReadOnlyCollection<ClaimsIdentity> col = handler.ValidateToken(token);
                ClaimsPrincipal principal = new ClaimsPrincipal(col[0]);
                Thread.CurrentPrincipal = principal;
                return base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("X509 validation has security token exception.");
                Trace.TraceError(ex.Message);
                statusCode = HttpStatusCode.Unauthorized;
            }

            return Task<HttpResponseMessage>.Factory.StartNew(() =>
                 new HttpResponseMessage(statusCode));
        }
    }
}
