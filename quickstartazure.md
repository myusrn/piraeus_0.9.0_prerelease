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
- [ ] Provision a Virtual Machine (VM) in Azure using the "Windows Server 2016 with Containers" SKU and choose the default 4-core VM size.

**Task 2: Configure the Piraeus Virtual Machine Host in Azure**

This task configures the VM by install Docker Compose and an External Virtual Switch in Hyper-V.

- [ ] Login (RDP) into the VM once it is provisioned in Azure and open Powershell As Administrator.
- [ ] Run the following command in Powershell to install Docker Compose.
```<language>
    Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe
```
- [ ] Run the following command in Powershell to install an External Virtual Switch in Hyper-V
```<language>
    New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true
```

**Task 3:  Download Docker Images**

This task will allow you to download the Docker images onto your Azure VM.
- [ ] Type the following commands in Powershell one at a time to pull the images from the Docker respository
```<language>
    docker pull skunklab/orleans-silo
    docker pull skunklab/tcpudpgateway
    docker pull skunklab/webgateway
```
- [ ] Type the following command in Powershell to very you have the images
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
- [ ] Save the file as "gateway-config.env" in the same folder that Powershell command prompt is pointing.

**Task 5:  Deploy**

-[ ] Type the following comnand in Powershell

  ```<language>
    docker-compose up -d
```
-[ ] The deployment should take approx. 2 minutes.  You can type "docker logs orleans-silo" in Powershell to check the progress.
-[ ] Remember to copy either the IP address or hostname of the VM to connect to it to run the samples.


Please see [Quick Start Running Client Sample] (quickstartclientsample.md) for running the client samples.