using RestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "Account")]
    public class AddAccount : Cmdlet
    {
        public string ServiceUrl { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            //string token = UserToken.Get(this.Key, this.ServiceUrl);

            //string url = String.Format("{0}/api/acct?name={1}", this.ServiceUrl, this.Name);

            //RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, true, token);
            //RestRequest request = new RestRequest(builder);
            //string result = request.Post();
            //WriteObject(result);
        }
    }


    [Cmdlet(VerbsCommon.Remove, "Account")]
    public class RemoveAccount : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }
        

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/acct", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            request.Delete();
        }
    }

    //[Cmdlet(VerbsData.Update, "Account")]
    //public class UpdateAccount : Cmdlet
    //{
    //    public string ServiceUrl { get; set; }

    //    public string Key { get; set; }

    //    public string Namespace { get; set; }

    //    protected override void ProcessRecord()
    //    {

    //    }
    //}

    [Cmdlet(VerbsCommon.Get, "Account")]
    public class GetAccount : Cmdlet
    {
        public string ServiceUrl { get; set; }

        public string Key { get; set; }

        protected override void ProcessRecord()
        {
            string token = UserToken.Get(this.Key, this.ServiceUrl);

            string url = String.Format("{0}/api/acct", this.ServiceUrl);

            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, token);
            RestRequest request = new RestRequest(builder);
            string result = request.Get<string>();
            WriteObject(result);
        }
    }



   
}
