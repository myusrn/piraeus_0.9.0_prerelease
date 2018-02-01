
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

$hostname="BLOB_STORAGE_HOSTNAME"  #If the blob storage endpint is "https://piraeusstore.blob.core.windows.net/" use "piraeusstore" as the hostname
$containerName="BLOB_STORAGE_CONTAINER"
$blobConnectionString="BLOB_STORAGE_CONNECTION_STRING"

Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token -ResourceUriString $resource_A  -BlobType Block -Host $hostname -Container $containerName -Key $blobConnectionString 
