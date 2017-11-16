using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Capl.Module
{
    [Cmdlet("Delete", "CaplPolicy")]
    public class CaplPolicyDelete : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
        public string Url;

        [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Policy ID used to return the CAPL policy.", Mandatory = true)]
        public string PolicyID;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/policy?policyId={1}", this.Url, this.PolicyID);
            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, "application/json", true, this.SecurityToken);
            RestRequest request = new RestRequest(builder);
            request.Delete();

        }
    }
}
