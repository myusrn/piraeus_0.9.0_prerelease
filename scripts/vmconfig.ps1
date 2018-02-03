param([string]$store1, [string]$key1, [string]$store2, [string]$key2)

#1. Install Docker Compose
#2. Download YML and ENV files from Git repository
#3. Download PS script to configure YML file
#4. Run PS script and configure YML file
#5. Install External Virtual Switch in Hyper-V
#6. Pull docker images for Piraeus 
#	a. The orleans-silo and tcpudpgateway download quickly
#	b. The webgateway images is contain a large layer for ASPNET, which is why the install is relatively lengthy)
#7. Run Docker Compose
#8. Restarts VM (necessary to reset the External Virtual Switch added)

#On restart Piraeus will be running in approx. 2 minutes
#Navigate to the Web Gateway http://ipaddress and it will indicate whether it is running



#Install the Azure Powershell Module 
Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
Install-Module AzureRM -AllowClobber -Force
Import-Module AzureRM

#create a folder for files
cd \
mkdir piraeus
cd piraeus


#storage connections string for later use with YML file
$connectionstring1 = "DefaultEndpointsProtocol=https;AccountName=" + $store1 + ";AccountKey=" + $key1 
$connectionstring2 = "DefaultEndpointsProtocol=https;AccountName=" + $store2 + ";AccountKey=" + $key2 

#yml file location
$ymlFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/docker-compose-azure.yml"

#env file location
$envFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/gateway-config.env"


#script to configure yml file
$psYmlConfigFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/scripts/ymlupdate.ps1"


#Install Docker Compose
Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe


#Download docker-compose.yml file
Invoke-WebRequest -Uri $ymlFileUrl -OutFile "docker-compose.yml"

#Download gateway-config.env file
Invoke-WebRequest -Uri $envFileUrl -OutFile "gateway-config.env"


#Download PS file to configure yml file
Invoke-WebRequest -Uri $psYmlConfigFileUrl -OutFile "ymlconfig.ps1"

#Add the storage account container for orleans grain state
$context = New-AzureStorageContext -StorageAccountName $store1 -StorageAccountKey $key1 -Protocol Https
New-AzureStorageContainer -Name "orleans" -Context $context

#Add the storage account container for the sample
$context = New-AzureStorageContext -StorageAccountName $store2 -StorageAccountKey $key2 -Protocol Https
New-AzureStorageContainer -Name "resource-a" -Context $context

#Configure the YML file
$argsList = "$connectionstring1 $connectionstring2"
$scriptPath = ".\ymlconfig.ps1"
Invoke-Expression "$scriptPath $argsList"


#Install External Virtual Switch
New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true

#Pull docker images for Piraeus 
docker pull skunklab/orleans-silo
docker pull skunklab/tcpudpgateway
docker pull skunklab/webgateway


#Run docker compose
docker-compose up -d

#Restart the VM
Restart-Computer




