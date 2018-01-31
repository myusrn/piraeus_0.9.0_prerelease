Quick Start
===========

The quick start is designed to get you running locally or in Azure.

Prerequisites for Local Machine
---------------------------------

-   Windows 10 or Windows Server 2016
-   [Install and configure docker for Windows](https://docs.microsoft.com/en-us/virtualization/windowscontainers/quick-start/quick-start-windows-10)
-   Switch Windows Containers (see above)
-  Install Docker Compose
-   Powershell
-   Visual Studio 2017
-   [Clone source github](https://github.com/skunklab/piraeus_0.9.0_prerelease)
-  Hyper-V 
-  External Virtual Switch

Running Locally in Docker
---------------
**Tasks**
-   Configure Environment
-   Create Azure Storage Accounts
-   Build the source
-   Deploy to local Docker machine
-   Run the sample
###
**Step 1:  Configure Environment**
Make sure all the prerequisites are installed.  You are going to need to add an External Virtual Switch to Hyper-V if you do not have one installed.  You check to see if one is installed. Open Powershell as administrator and execute the following command:

    Get-VMSwitch
  If you have a SwitchType of "External" move to Step 2; otherwise execute the following command in Powershell
  

    Get-NetAdapter | Select Name, InterfaceDescription

Find a network adapter "Name" associated with Hyper-V, e.g., Hyper-V Virtual Ethernet Adapter.  Execute the following command in Powershell replacing the Name of the network adapter found previously "NAME_OF_ADAPTER" in the command.

    New-VMSwitch -Name "DockerSwitch" -SwitchType External -AllowManagement $true -NetAdapterName "NAME_OF_ADAPTER"

**Step 2:  Create Azure Storage Accounts**
You will need to go to the Azure portal and create 2 azure storage accounts.  One account is used by Orleans for managing the cluster and state.  The other for the samples.  ***Create a container in blob storage named "grains" for the Orleans storage account.***  Make sure to copy the storage account connection strings, because we will use these later. 

**Step 3:  Build the Source**
Open a Visual Studio 2017 command prompt and navigate to the "build" folder.  Type "build" on the command line to build the source.  Note:  It may be helpful to keep and instance of Visual Studio open while building the source.

**Step 4:  Deploy to Docker**
Using the Visual Studio command prompt navigate to the /src/Docker folder.
You will need to edit the following environment variables in the docker-compose.yml file with the storage connection strings related to Orleans and the samples, i.e. Piraeus using a text editor.    

ORLEANS_LIVENESS_DATACONNECTIONSTRING=#ORLEANS_STORAGE_CONNECTIONSTRING
ORLEANS_PROVIDER_DATACONNECTIONSTRING=#ORLEANS_STORAGE_CONNECTIONSTRING
ORLEANS_AUDIT_DATACONNECTIONSTRING=#PIRAEUS_STORAGE_CONNECTIONSTRING
  

Type the following on the command line in Visual Studio 2017 command prompt.  This will create the local docker images.

    powershell local-docker-build

Type the following on the command line and deploy the containers and configuration to docker, which will also start the containers.

    docker-compose up -d

The start up process should take approx. 2 minutes.  You can check the progress by typing the following on the command line.

    docker logs orleans-silo

**Step 5:  Running the Sample**
You will need to get the docker IP address of both the Web and TCP servers.  On the command line type

    docker inspect web-server
and the IP Address of the Web server should be visible in the output.  Type

    docker inspect tcp-server 
  and get the IP Address of the TCP server.

Open a Powershell ISV instance and load the file SamplesConfig.ps1 in the /scripts folder.  Run the file in the Powershell ISV.  This will configure 2 CAPL authorization policies and 2 Piraeus resources, i.e.. resource-a and resource-b.  Users in the role "A" can publish to resource-a and subscribe to resource-b.  Users in the role "B" can publish to resource-b and subscribe to resource-a.

We need to start some clients to begin communications with Piraeus.  The /src/Samples folder container 3 clients, CoAP, MQTT, and Rest.  To open the CoAP client navigate to /src/samples/samples.clients.coap/samples.clients.coap/bin/release and double-click the Samples.Clients.Coap.exe.  Similarly, go to the /bin/release folder of the MQTT client and double-click the Samples.Clients.Mqtt.exe
 






