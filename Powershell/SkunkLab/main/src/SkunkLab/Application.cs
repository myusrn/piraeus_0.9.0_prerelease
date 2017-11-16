
using SkunkLab.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab
{

    [Cmdlet(VerbsCommon.Add, "Application")]
    public class AddApplication : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        public string Id { get; set; }
        public string Description { get; set; }

        public ClaimLiteral[] Claims { get; set; }

        protected override void ProcessRecord()
        {            
            AppIdentity identity = new AppIdentity(this.Id, this.Description, this.Claims);

            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/app", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, token);
            RestRequest request = new RestRequest(builder);
            request.Post<AppIdentity>(identity);
        }
    }

    [Cmdlet(VerbsCommon.Remove, "Application")]
    public class RemoveApplication : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        public string Id { get; set; }

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/app?id={0}", this.ServiceUrl, this.Id);

            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();
        }
    }

    [Cmdlet(VerbsData.Update, "Application")]
    public class UpdateApplication : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public ClaimLiteral[] Claims { get; set; }

        protected override void ProcessRecord()
        {
            AppIdentity identity = new AppIdentity(this.Id, this.Description, this.Claims);

            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/app", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Json, false, token);
            RestRequest request = new RestRequest(builder);
            request.Put<AppIdentity>(identity);
        }
    }


    [Cmdlet(VerbsCommon.Get, "Application")]
    public class GetApplication : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        public string Id { get; set; }

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/app?id={1}", this.ServiceUrl, this.Id);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            AppIdentity identity = request.Get<AppIdentity>();
            WriteObject(identity);
        }
    }


    [Cmdlet(VerbsCommon.Get, "ApplicationsList")]
    public class ListApplication : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/app/list", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            AppIdentity[] identities = request.Get<AppIdentity[]>();

            WriteObject(identities);
        }
    }

    

}
