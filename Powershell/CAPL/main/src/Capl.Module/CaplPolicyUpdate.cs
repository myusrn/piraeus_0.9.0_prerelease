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
    [Cmdlet("Update", "CaplPolicy")]
    public class CaplPolicyUpdate : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
        public string Url;

        [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "CAPL Policy to update.", Mandatory = true)]
        public AuthorizationPolicy Policy;

        protected override void ProcessRecord()
        {
            RestRequestBuilder builder = new RestRequestBuilder("PUT", this.Url, "application/xml", false, this.SecurityToken);
            RestRequest request = new RestRequest(builder);
            request.Put<AuthorizationPolicy>(this.Policy);
        }
    }
}
