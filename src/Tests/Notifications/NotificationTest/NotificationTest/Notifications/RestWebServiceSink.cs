using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NotificationTest.Notifications
{
    public class RestWebServiceSink : EventSink
    {
        public RestWebServiceSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            tokenType = metadata.TokenType;
            symmetricKey = metadata.SymmetricKey;

            Uri uri = new Uri(metadata.NotifyAddress);

            
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            issuer = nvc["issuer"];
            audience = nvc["audience"];
            nvc.Remove("issuer");
            nvc.Remove("audience");

            string uriString = nvc.Count == 0 ? String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.LocalPath) :
                String.Format("{0}{1}{2}{3}?", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.LocalPath);

            StringBuilder builder = new StringBuilder();
            builder.Append(uriString);
            for (int i = 0; i < nvc.Count; i++)
            {
                string key = nvc.GetKey(i);
                string value = nvc[key];
                builder.Append(String.Format("{0}={1}", key, value));
                if (i < nvc.Count - 1)
                    builder.Append("&");

            }

            address = builder.ToString();
        }

        private Piraeus.Core.Metadata.SecurityTokenType? tokenType;
        private string symmetricKey;
        private string issuer;
        private string audience;
        private string address;
        private string token;
        private DateTime tokenExpiration;


        private void SetSecurityToken(HttpWebRequest request)
        {
            if (!metadata.TokenType.HasValue || metadata.TokenType.Value == Piraeus.Core.Metadata.SecurityTokenType.None)
                return;

            if(metadata.TokenType.Value == Piraeus.Core.Metadata.SecurityTokenType.X509)
            {
                throw new NotImplementedException("X509 client certificates not yet available to use.");
            }
            else
            {
                string token = null;
                if (metadata.TokenType.Value == metadata.TokenType)
                {
                    JsonWebToken jwt = new JsonWebToken(metadata.SymmetricKey, null, 20.0, issuer, audience);
                    token = jwt.ToString();
                    tokenExpiration = DateTime.UtcNow.AddMinutes(19.0);


                    
                    //JsonWebToken token = new JsonWebToken(null, symmetricKey, issuer, config.Identity.Service.Claims);
                    //request.Headers.Add("Authorization", String.Format("Bearer {0}", token.ToString());
                }
                else
                {

                    //SimpleWebToken swt = new SimpleWebToken(issuer, audience, claims);
                    //swt.Sign(Convert.FromBase64String(symmetricKey));
                    //token = swt.ToString();
                }

                request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
            }
        }

        public override async Task SendAsync(EventMessage message)
        {
            HttpWebRequest request = null;

            try
            {
                request = HttpWebRequest.Create(address) as HttpWebRequest;
                request.ContentType = message.ContentType;
                request.Method = "POST";

                SetSecurityToken(request);

                request.ContentLength = message.Message.Length;
                Stream stream = await request.GetRequestStreamAsync();
                await stream.WriteAsync(message.Message, 0, message.Message.Length);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw;
            }

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Trace.TraceInformation("Rest request is success.");
                    }
                    else
                    {
                        Trace.TraceInformation("Rest request return an expected status code.");
                    }
                }
            }
            catch (WebException we)
            {
                string faultMessage = String.Format("subscription '{0}' with status code '{1}' and error message '{2}'", metadata.SubscriptionUri, we.Status.ToString(), we.Message);
                Trace.TraceError(faultMessage);
            }
        }
    }
}
