
using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "SkunkLabSecurityKey")]
    public class SkunkLabSecurityKeyAdd : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Determines whether the key has admin rights.", Mandatory = true)]
        public bool Admin;

        [Parameter(HelpMessage = "(Optional) Description for the key.", Mandatory = false)]
        public string Description;

        [Parameter(HelpMessage = "Claims required for non-admin", Mandatory = false)]
        public Claim[] Claims;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            List<Piraeus.Services.Common.ClaimLiteral> literals = new List<ClaimLiteral>();
            foreach (Claim claim in this.Claims)
            {
                literals.Add(new ClaimLiteral(claim.Type, claim.Value));
            }

            if (literals.Count == 0)
            {
                literals = null;
            }

            string url = String.Format("{0}/api/securitykey", this.ServiceUrl);
            
            App app = new App(this.AppId, this.Description, literals.ToArray());

            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, token);
            RestRequest request = new RestRequest(builder);
            string result = request.Post<App, string>(app);
            WriteObject(result);
        }
    }
}
