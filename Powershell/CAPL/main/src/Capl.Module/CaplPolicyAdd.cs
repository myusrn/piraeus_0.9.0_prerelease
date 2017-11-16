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
    [Cmdlet(VerbsCommon.Add, "CaplPolicy")]
    public class CaplPolicyAdd : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory=true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "CAPL Policy to load.", Mandatory = true)]
        public AuthorizationPolicy Policy;
                

        protected override void ProcessRecord()
        {
            RestRequestBuilder builder = new RestRequestBuilder("POST", this.ServiceUrl, "application/xml", false, this.Key);
            RestRequest request = new RestRequest(builder);
            request.Post<AuthorizationPolicy>(this.Policy);
        }
    }
}
