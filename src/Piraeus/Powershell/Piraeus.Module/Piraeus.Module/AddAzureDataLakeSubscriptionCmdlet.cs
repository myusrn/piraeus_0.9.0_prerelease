using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusDataLakeSubscription")]
    public class AddAzureDataLakeSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Host name of CosmosDb, e.g, <host>.documents.azure.com:443", Mandatory = true)]
        public string Host;

        [Parameter(HelpMessage = "Application ID for access from AAD.", Mandatory = true)]
        public string ApplicationId;

        [Parameter(HelpMessage = "Tenant for access from AAD, e.g., contoso.onmicrosoft.com", Mandatory = true)]
        public string Tenant;

        [Parameter(HelpMessage = "Name of folder to write data.", Mandatory = true)]
        public string Folder;

        [Parameter(HelpMessage = "Security key (secret) from AAD for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("adl://{0}.azuredatalakestore.net?appid={1}&tenantId={2}&folder={3}", Host, ApplicationId, Tenant, Folder);

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
