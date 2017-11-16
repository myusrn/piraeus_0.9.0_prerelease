using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Remove, "SkunkLabAccount")]
    public class SkunkLabAccountRemove : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/acct", this.ServiceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();
        }
    }
}
