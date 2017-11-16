using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Remove, "SkunkLabApp")]
    public class SkunkLabAppRemove : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "The unique ID of the applicaition.", Mandatory = true)]
        public string AppId;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;


        protected override void ProcessRecord()
        {
            string token = UserToken.Get(Key, ServiceUrl);
            string url = String.Format("{0}/api/app?appId={1}", this.ServiceUrl, this.AppId);

            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();

        }
    }
}
