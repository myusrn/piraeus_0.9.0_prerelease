using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusCosmosDbSubscription")]
    public class AddAzureCosmosDbSubscriptonCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Host name of CosmosDb, e.g, <host>.documents.azure.com:443", Mandatory = true)]
        public string Host;

        [Parameter(HelpMessage = "Name of database.", Mandatory = true)]
        public string Database;

        [Parameter(HelpMessage = "Name of collection.", Mandatory = true)]
        public string Collection;

        [Parameter(HelpMessage = "CosmosDb read-write key", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("https://{0}.documents.azure.com:443?database={1}&collection={2}", Host, Database, Collection);

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
