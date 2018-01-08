//using Microsoft.Rest.Azure.Authentication;
//using Piraeus.Core.Messaging;
//using Piraeus.Core.Metadata;
//using System;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using System.Web;

//namespace Piraeus.Grains.Notifications
//{
//    public class DataLakeSink : EventSink
//    {

//        private string appId;
//        private string secret;
//        private string tenantId;
//        private string fqdn;
//        private string folder;
//        private AdlsClient client;
//        private string subscriptionUriString;

//        /// <summary>
//        /// Creates Azure Data Lake notification 
//        /// </summary>
//        /// <param name="contentType"></param>
//        /// <param name="messageId"></param>
//        /// <param name="metadata"></param>
//        /// <remarks>adl://host.azuredatalakestore.net?appid=id&tenantid=id&secret=token&folder=name</remarks>
//        public DataLakeSink(SubscriptionMetadata metadata)
//            : base(metadata)
//        {
//            try
//            {
//                subscriptionUriString = metadata.SubscriptionUriString;

//                Uri uri = new Uri(metadata.NotifyAddress);
//                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
//                appId = nvc["appid"];
//                tenantId = nvc["tenantid"];
//                folder = nvc["folder"];
//                secret = metadata.SymmetricKey;

//                //adl://malong.azuredatalakestore.net
//                string fqdn = String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.AbsolutePath);
//                var creds = new ClientCredential(appId, secret);

//                //var clientCreds = ApplicationTokenProvider.LoginSilentAsync(tenantId, creds).GetAwaiter().GetResult();
//                Task<ServiceClientCredentials> task = LoginSilent(creds);
//                var clientCreds = task.Result;
//                client = AdlsClient.CreateClient(fqdn, clientCreds);
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Azure data lake subscription client not created for {0}", subscriptionUriString);
//                Trace.TraceError("Azure data lake subscription {0} ctor error {1}", subscriptionUriString, ex.Message);
//            }
//        }

//        private Task<ServiceClientCredentials> LoginSilent(ClientCredential creds)
//        {
//            TaskCompletionSource<ServiceClientCredentials> tcs = new TaskCompletionSource<ServiceClientCredentials>();
//            Task<ServiceClientCredentials> t = ApplicationTokenProvider.LoginSilentAsync(tenantId, creds);
//            tcs.SetResult(null);
//            return tcs.Task;
//        }

//        public override async Task SendAsync(EventMessage message)
//        {
//            try
//            {
//                string filename = String.Format("/{0}/{1}", folder, GetFilename(message.ContentType));
//                using (AdlsOutputStream stream = await client.CreateFileAsync(filename, IfExists.Overwrite))
//                {
//                    await stream.WriteAsync(message.Message, 0, message.Message.Length);
//                    await stream.FlushAsync();
//                    stream.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError("Azure data lake subscription {0} writing error {1}", subscriptionUriString, ex.Message);
//            }
//        }

//        private string GetFilename(string contentType)
//        {
//            string suffix = null;
//            if (contentType.Contains("text"))
//            {
//                suffix = "txt";
//            }
//            else if (contentType.Contains("json"))
//            {
//                suffix = "json";
//            }
//            else if (contentType.Contains("xml"))
//            {
//                suffix = "xml";
//            }

//            string filename = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss-fffff");
//            return suffix == null ? filename : String.Format("{0}.{1}", filename, suffix);
//        }
//    }
//}
