
#Configure a Storage Account to Receive messages


#IMPORTANT:  Import the Piraeus Powershell Module
import-module #FULL PATH to Piraeus.Module.dll  located in src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll

#Login to the Management API

#URL of the Piraeus Web Gateway
#If running in Azure use the hostname or IP address of the virtual machine
#If running locally, type "docker inspect webgateway" to obtain the IP address of the web gateway

$url = "http://HostNameOrIPAddress"  #Replace with Host name or IP address of the Piraeus Web Gateway


#get a security token for the management API
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"


$resource_A = "http://www.skunklab.io/resource-a"
$containerName="resource-a"

$storageAcct="BLOB_STORAGE_ACCT"  #If the blob storage endpint is "https://pirstore.blob.core.windows.net/" use "pirstore" as the hostname
$storageKey="BLOB_STORAGE_KEY"  #Security key to blob storage account

Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token -ResourceUriString $resource_A  -BlobType Block -Host $hostname -Container $containerName -Key $key 
