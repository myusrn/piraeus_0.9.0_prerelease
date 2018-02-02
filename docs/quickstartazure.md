Quick Start Deploying Piraeus in Azure
===========


---------------
**Tasks**
- Provision Environment in Azure
-   Configure the Piraeus Virtual Machine Host in Azure
-   Dowloand Docker Images
-   Configure Docker Compose
-   Deploy

###
**Task 1:  Provision Environment in Azure**

The following steps will create the resources in Azure needed to run Piraeus in Azure.

- [ ] Create a storage account in Azure.  In the Blob service create a container and name it "orleans".  Copy the Blob storage connection string for later use.  We will call this connection string, CONNECTION_STRING_A.
- [ ] Create another storage account in Azure.  Copy the Blob storage connection string for later use.  We will call this connection string, CONNECTION_STRING_B.
- [ ] Provision a Virtual Machine (VM) in Azure using the "**Windows Server 2016 DataCenter - with Containers**" SKU and choose the **D4S_V3** VM size.
- [ ] You will need to add 2 network security groups (NSGs) such that inbound ports will be open to recieve connections from the clients.  You will need to add an inbound port rule for port 80 and an inbound port rule for a the port range 5683-8883 to speed the process.  Below is an image that shows the configuration.
![](https://pegasusmission.files.wordpress.com/2018/02/nsg.png) 

**Task 2: Configure the Piraeus Virtual Machine Host in Azure**

This task configures the VM by installing Docker Compose and an External Virtual Switch in Hyper-V.

- [ ] Login (RDP) into the VM once it is provisioned in Azure and open Powershell As Administrator.
- [ ] Run the following command in Powershell to install Docker Compose.
```<language>
    Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe
```
- [ ] Run the following command in Powershell to install an External Virtual Switch in Hyper-V
```<language>
    New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true
```
- [ ] You will also need to reinitialize Hyper-V for the External Virtual Switch just added.  Do this by closing the RDP session and going back to the Azure portal and restarting the VM.  Once the VM restarts, login (RDP) again into the VM to continue and open Powershell As Administrator again.

**Task 3:  Download Docker Images**

This task will allow you to download the Docker images onto your Azure VM.
- [ ] Type the following commands in Powershell one at a time to pull the images from the Docker respository. 
```<language>
    docker pull skunklab/orleans-silo
    docker pull skunklab/tcpudpgateway
    docker pull skunklab/webgateway
```
- [ ] Type the following command in Powershell to verify you have the images
```<language>
    docker images
```


**Task 4:  Configure Docker Compose**
This task will allow you to configure the deployment.
- [ ] Go to the folder /src/docker in the source code and copy the contents of the docker-compose-azure.yml file.
- [ ] Open notepad in the Azure VM and paste in the copied contents.
- [ ] In notepad, replace the following environment variables with the storage account connection string "CONNECTION_STRING_A"
```<language>
   ORLEANS_LIVENESS_DATACONNECTIONSTRING=your_connection_string
   ORLEANS_PROVIDER_DATACONNECTIONSTRING=your_connection_string
```
- [ ] In notepad, replace the following environment variable with the storage acocunt connection string  "CONNECTION_STRING_B"
```<language> 
   ORLEANS_AUDIT_DATACONNECTIONSTRING=your_connection_string
```
- [ ] Save the file as "docker-compose.yml" in the same folder that the Powershell command prompt is pointing.
- [ ] Go to the folder /src/docker in the source code and copy the contents of the gateway-config.env file.
- [ ] Open notepad in the Azure VM and paste in the copied contents.
- [ ] Save the file as "gateway-config.env" in the same folder that the Powershell command prompt is pointing to.

**Task 5:  Deploy**

-[ ] Type the following comnand in Powershell

  ```<language>
    docker-compose up -d
```
-[ ] The deployment should take approx. 2 minutes.  You can type "docker logs orleans-silo" in Powershell to check the progress.  Once  the orleans-silo logs print "...running and blocking.", the silo has started correctly.

- [ ] You check the deployment by opening a browser and navigating to the public ip address of the VM.

- [ ] Remember to copy either the IP address or hostname of the VM to connect to it to run the samples.


Please see [Quick Start Running Client Sample] (https://github.com/skunklab/piraeus_0.9.0_prerelease/blob/master/quickstartclientsample.md) for running the client samples.
