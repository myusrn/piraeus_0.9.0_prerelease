using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "AzureBlobStorageSubscription")]
    public class AddAzureBlobStorageSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Host name of Azure Blob Storage, e.g, <host>.blob.core.windows.net", Mandatory = true)]
        public string Host;

        [Parameter(HelpMessage = "Name of container to write messages.  If omitted writes to $Root.", Mandatory = false)]
        public string Container;

        [Parameter(HelpMessage = "Type of blob(s) to create, i.e., block, page, append.", Mandatory = true)]
        public AzureBlobType BlobType;

        [Parameter(HelpMessage = "Either storage key or SAS token for container or account.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("https://{0}.blob.core.windows.net?container={1}&blobtype={2}", Host, Container, BlobType.ToString());

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = Key
            };


            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);

            
        }
    }
}
