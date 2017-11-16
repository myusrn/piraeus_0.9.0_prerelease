
using Piraeus.Services.Common;
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
    [Cmdlet(VerbsCommon.Add, "SkunkLabApp")]
    public class SkunkLabAppAdd : Cmdlet
    {       

        [Parameter(HelpMessage = "The unique ID of the applicaition.", Mandatory = true)]
        public string AppId;

        [Parameter(HelpMessage = "Description of the application.", Mandatory = true)]
        public string Description;

        [Parameter(HelpMessage = "Claims sent by users of the application.", Mandatory = true)]
        public Claim[] Claims;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            List<Piraeus.Services.Common.ClaimLiteral> literals = new List<ClaimLiteral>();
            foreach(Claim claim in this.Claims)
            {
                literals.Add(new ClaimLiteral(claim.Type, claim.Value));              
            }

            if(literals.Count == 0)
            {
                literals = null;
            }


            string url = String.Format("{0}/api/app", this.ServiceUrl);
            App app = new App(this.AppId, this.Description, literals.ToArray());
            
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, token);
            RestRequest request = new RestRequest(builder);
            string result = request.Post<App, string>(app);
            WriteObject(result);
        }
    }
}
