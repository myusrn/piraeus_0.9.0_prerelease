using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "SkunkLabNamespace")]
    public class SkunkLabNamespaceAdd : Cmdlet
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
            string url = String.Format("{0}/api/namespace?ns={1}", this.ServiceUrl, this.Namespace);

            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);

            request.Post();
        }

    }
}
