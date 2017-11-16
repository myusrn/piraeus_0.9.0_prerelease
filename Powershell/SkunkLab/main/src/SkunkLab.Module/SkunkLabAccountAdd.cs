using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "SkunkLabAccount")]
    public class SkunkLabAccountAdd : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Namespace, e.g. www.example.org", Mandatory = true)]
        public string Namespace;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api2/acct?ns={1}", this.ServiceUrl, this.Namespace);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, true, null);
            RestRequest request = new RestRequest(builder);
            string result = request.Post<string>();
            WriteObject(result);
        }
    }
}
