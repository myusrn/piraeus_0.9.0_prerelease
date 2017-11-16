using Piraeus.Services.Common;
using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Get, "SkunkLabApp")]
    public class SkunkLabAppGet : Cmdlet
    {
        [Parameter(HelpMessage = "The unique ID of the applicaition.", Mandatory = true)]
        public string AppId;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(Key, ServiceUrl);
            string url = String.Format("{0}/api/app?appId={1}", this.ServiceUrl, this.AppId);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            App app = request.Get<App>();
            WriteObject(app);
        }
    }
}
