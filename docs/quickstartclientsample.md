nQuick Start Running Client Sample
===========

Please see [Quick Start Deploying Piraeus in Azure] (quickstartazure.md) prior to performing this quick start.

---------------
**Tasks**
- Configure Piraeus with Powershell
-   Configure Blob Storage Receiver
-  Run the Sample Clients

###
**Task 1: Configure Piraeus with Powershell **

The following steps will configure Piraeus resources needed to run the sample clients.

- [ ] On your local machine open Powershell ISV and open the file /scripts/SamplesConfig.ps1
- [ ] You will need to Import the Piraeus Powershell Module. Then replace the top line in the SamplesConfig.ps1 script that contains "Import-Module" with the full path to the Piraeus.Module.dll located in the  src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release folder. Example below
```<language>
Import-Module "c:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll"
```
- [ ] 
- [ ] Replace the following text with the host name or IP address of the VM in Azure running Piraeus, e.g., $url = "http://30.79.69.230"
```<language>
$url = "http://HostNameOrIPAddress" 
```
- [ ] Run the Powershell script in Powershell ISV.  This may be initially slow because the Orleans client in the Web Gateway must be initialized.

Piraeus is now configured to run the sample clients.

**Task 2: Configure Blob Storage Receiver**

This task uses Powershell to configure a Blob Storage Receiver with the sample clients.  

- [ ] Using the storage account created for "CONNECTION_STRING_B" in the Quick start, add a new container named "resource-a".
- [ ] Open the Powershell script /scripts/SampleConfigureBlobStorageReceiver.ps1 in Powershell ISV
- [ ] Replace the following text with the host name or IP address of the VM in Azure running Piraeus, e.g., $url = "http://30.79.69.230"
```<language>
$url = "http://HostNameOrIPAddress" 
```
- [ ] Replace the following text with the host name of the blob storage account, e.g., use the text "piraeusstore" if the endpoint is "https://piraeusstore.blob.core.windows.net/"
```<language>
$hostname="BLOB_STORAGE_HOSTNAME"
```
- [ ] Replace the following text with the container name you created, i.e., "resource-a".
```<language>
$containerName="BLOB_STORAGE_CONTAINER"
```
- [ ] Replace the following text with the storage account connection string related to "CONNECTION_STRING_B"
```<language>
$blobConnectionString="BLOB_STORAGE_CONNECTION_STRING"
```
- [ ] Run the Powershell script in Powershell ISV

This script has configured the blob storage container to receive any message sent to "resource-a" by any client.

**Task 3: Run the Sample Clientsr**
There are 3 sample clients, which are console applications located in the /src/Samples folder.  Each client takes a "role", i.e. A or B,
which implies that role "A" can only send to "resource-a" and only receive from "resource-b".  The converse is true for a client in role "B".
There are 3 protocol clients 
- [ ] CoAP client /src/Samples/Clients/Samples.Clients.Coap/bin/Release/Samples.Clients.Coap.exe
- [ ] MQTT client /src/Samples/Clients/Samples.Clients.Mqtt/bin/Release/Samples.Clients.Mqtt.exe
- [ ] REST client /src/Samples/Clients/Samples.Clients.Rest/bin/Release/Samples.Clients.Rest.exe

You can check the blob storage and see the messages sent from any client in role "A".
