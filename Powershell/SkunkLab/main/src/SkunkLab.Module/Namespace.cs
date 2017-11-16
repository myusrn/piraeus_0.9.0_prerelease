using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{

    [Cmdlet(VerbsCommon.Add, "Namespace")]
    public class AddNamespace : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Namespace, e.g. www.example.org", Mandatory = true)]
        public string Namespace;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/namespace?name={1}", this.ServiceUrl, this.Name);

            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Post();
        } 
    }

    [Cmdlet(VerbsCommon.Remove, "Namespace")]
    public class RemoveNamespace : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Namespace, e.g. www.example.org", Mandatory = true)]
        public string Namespace;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/namespace?name={1}", this.ServiceUrl, this.Name);

            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();
        }
    }

    [Cmdlet(VerbsCommon.Get, "Namespace")]
    public class GetNamespace : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Namespace, e.g. www.example.org", Mandatory = true)]
        public string Namespace;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/namespace?name={1}", this.ServiceUrl, this.Name);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            string result = request.Get<string>();
        }
    }


    [Cmdlet(VerbsCommon.Get, "NamespaceList")]
    public class NamespaceList : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/namespace", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            string[] result = request.Get<string[]>();
        }
    }
}
