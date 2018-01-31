##Welcome to the Skunk Lab
![](/docs/images/skunklab.jpg "")

##What is Piraeus?
Piraeus simplifies communications for heterogeneous open-systems by providing a platform for low latency and linearly scalable communications.  It allows subsystems to communicate bidirectionally and symmetrically without dependency on the channels and protocols used by other subsystems.  Piraeus supports all cardinalities and simplifies dispatch operations without headover. It is as reactive as a online video game.  You read more here about its origins and the many features provided to simplify open-system communications.


There are 3 tasks needed to get started
1. Configure machine
2. Start docker containers
3. Run the sample


##Task 1: Configure Machine
Configuring the machine depends on whether it is your local machine or an Azure virtual machine.

###Local Machine Option

- Add Hyper-V feature (if not present)
- Create External Virtual Switch
- Install Docker for Windows and Switch to Windows Containers

###Azure Virtual Machine Option
- Create Azure Virtual Machine with containers
- Install Docker Compose
- Create External Virtual Switch


## Task 2 : Start Docker Containers
###Step 1: Pull the images from the docker respository
Download images from Docker respository by opening Powershell and typing the following docker pull commands from a Powershell command prompt.

`docker pull skunklab/orleans-silo`

`docker pull skunklab/tcpudpgateway`

`docker pull skunklab/webgateway`

###Step 2: Storage Accounts
Create 2 storages accounts in Azure, one for the Orleans cluster management and grain state, the other for our use in the sample.  Save the connection string for each storage account.

Open to the storage account you created for the Orleans cluster management in the Azure Portal, then open Blob Services.  Add a container named "orleans".  This will be used by Orleans to manage the grain state.

###Step 3: Configure docker-compose.yml file
We are going to use Docker Compose to configure and start our containers.  We will need to edit the docker-compose.yml found in the /src/docker folder.

Open the docker-compose.yml file edit the following environment variables for the orleans-silo container.  Replace the 2 environment variable values  

`#ORLEANS_BLOB_CONNECTIONSTRING`

with the blob storage connection string you created for Orleans cluster management. Replace the environment variable value

`#SAMPLE_BLOB_CONNECTIONSTRING`

with the other blob storage connection string and save the docker-compose.yml file.

###Step 4: Start the Docker Containers
Using the command line in the /src/docker folder type

`docker-compose up -d`

##Task 3: Running the Sample
You will need to configure Piraeus resources and access control policies to communicate.  This requires access to the Piraeus Management API, which we will use through Piraeus Powershell module using Powershell ISV.

###Step 1 : Configuring Piraeus
Open the Powershell ISV tool and load the file "/scripts/SamplesConfig.ps1".  You will notice that the first instruction is an Import-Module

`import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll`

You will need to replace this path with the path to the Piraeus.Module.dll located in your source.

###Step 2: Configure IP Address of Piraeus
You will need to replace the IP address of the $url variable (below) in the script with the IP address of either webgateway container if you are running on your local machine, OR the IP address of the Azure virtual machine if you are running in Azure.

`$url = "http://40.79.69.230"`

Below are the steps for each.

####Local Machine webgateway container IPAdress
Type the following of the command line in Powershell.  This will list all the properties of the webgateway container.  

`docker insepct web-server`

Find the IPAddress property and copy the corresponding IP address.

####Azure Virtual Machine IP Address
Navigate to the virtual machine in the Azure, then find and copy its public IP address.

Once you have completed these tasks, updating the path to the Piraeus.Module.dll and updating the IP address, run the SampleConfig.ps1 script in the Powershell ISV.

---You have configured Piraeus and are ready to start communications ----

###Step 3: Open any of the client samples in /src/Samples/Clients, e.g. CoAP, MQTT, or REST and follow the guidance instructions in the console apps.  

####Note: When you select a "role", e.g., A or B, it is important to note that role A can communicate with any number of clients in role B, the converse also holds.

####Note:  Local Machine Deployment for Piraeus
The IP address previously obtained will connect only to HTTP endpoints, e.g., REST and Web sockets.  If you want to use the TPC or UDP endpoints you will to run

`docker inspect tcp-server`

to obtain the IP address of the tcp/udp server to use with the client samples.

####Note: Azure Virtual Machine Deployments
The IP address previously obtained can be used with all channel types, i.e., HTTP, Web socket, TCP, and UDP

###Step 4: Adding a Passive Reciever (Blob Storage)
You can add many other options for passive receivers, e.g., Blob storage.  Using the Powershell ISV tool you can add the following line at the bottom of the SamplesConfig.ps1.  Create a container in the Azure blob storage account created for the sample and complete the 3 variables below.

`$host=YOUR_BLOB_STORAGE_HOSTNAME //e.g. in piraeus.blob.core.windows.net PIRAEUS is the host name`

`$containerName=BLOB_STORAGE_CONTAINER`

`$blobConnectionString=CONNECTION_STRING`

`Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token -ResourceUriString $resource_A  -BlobType Block -Host $host -Container $containerName -Key $blobConnectionString`

Run in Powershell ISV and when a client in the "A" role sends a message it will get put in the Blob Storage container you provider.




































