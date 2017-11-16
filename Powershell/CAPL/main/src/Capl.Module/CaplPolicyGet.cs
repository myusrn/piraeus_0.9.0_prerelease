using Capl.Authorization;
using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Capl.Module
{
    [Cmdlet(VerbsCommon.Get, "CaplPolicy")]
    public class CaplPolicyGet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
        public string Url;

        [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Policy ID used to return the CAPL policy.", Mandatory = true)]
        public string PolicyID;
        
        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/policy?PolicyId={1}", this.Url, this.PolicyID);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, "application/xml", true, this.SecurityToken);
            RestRequest request = new RestRequest(builder);
            WriteObject(request.Get<AuthorizationPolicy>());
        }
    }
}
