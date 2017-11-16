using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Capl.Module
{
    [Cmdlet(VerbsCommon.Find, "CaplPolicy")]
    public class CaplPolicyFind : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
        public string Url;

        [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
        public string SecurityToken;
        
        protected override void ProcessRecord()
        {
            RestRequestBuilder builder = new RestRequestBuilder("GET", this.Url, "application/json", true, this.SecurityToken);
            RestRequest request = new RestRequest(builder);
            WriteObject(request.Get<string[]>());
        }
    }
}
