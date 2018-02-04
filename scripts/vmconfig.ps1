param([string]$store1, [string]$key1, [string]$store2, [string]$key2)

#1. Install Azure Powershell Module
#2. Create folder "piraeus" to store files
#3. Download the docker-compose file
#4. Download the environment variable file used with docker compose file
#5. Create container in Orleans blob storage account and set the storage connnection string in docker-compose.yml file
#6. Create container in Sample blob storage account and set the storage connection string in docker-compose.yml file
#7. Install External Virtual Switch in Hyper-V
#8. Pull docker images from respository skunklab/orleans-silo, skunklab/tcpudpgateway, skunklab/webgateway
#9. Run docker-compose and deploy
#10. Restart the VM required in enable External Virtual Switch

#Note:  The Web gateway image is large; making the initial deployment 15-22 minutes.  
#Note:  When the VM is running you can open a browser go to http://ipaddress of the VM.
#       Check the "running" indicator on the weg gateway's home page
#       You are ready to run the sample!

#Dare Mighty Things :-)


#Install the Azure Powershell Module 
Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
Install-Module AzureRM -AllowClobber -Force
Import-Module AzureRM

#create a folder for files
cd \
mkdir piraeus
cd piraeus

$Acl = Get-Acl "C:\piraeus"
$Ar = New-Object  system.security.accesscontrol.filesystemaccessrule("Authenticated Users","FullControl","Allow")
$Acl.SetAccessRule($Ar)
Set-Acl "C:\piraeus" $Acl



#yml file location
$ymlFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/docker-compose-azure.yml"

#env file location
$envFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/gateway-config.env"


#Install Docker Compose
Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe


#Download docker-compose.yml file
Invoke-WebRequest -Uri $ymlFileUrl -UseBasicParsing -OutFile "docker-compose.yml" 

#Download gateway-config.env file
Invoke-WebRequest -Uri $envFileUrl -UseBasicParsing -OutFile "gateway-config.env" 



function UpdateYmlAndStore
{
    Param ([string]$acctName, [string]$storeKey, [string]$matchString, $containerName)

    $connectionString = "DefaultEndpointsProtocol=https;AccountName=" + $acctName + ";AccountKey=" + $storeKey

    $path = "docker-compose.yml"

    (Get-Content $path) -replace $matchString,$connectionString | out-file $path

    $context = New-AzureStorageContext -StorageAccountName $acctName -StorageAccountKey $storeKey -Protocol Https
    New-AzureStorageContainer -Name $containerName -Context $context
}


#Add the storage account container for orleans grain state
UpdateYmlAndStore -acctName $store1 -storeKey $key1 -matchString "#ORLEANS_BLOB_STORAGE_CONNECTIONSTRING" -containerName "orleans"

#Add the storage account container for the sample
UpdateYmlAndStore -acctName $store2 -storeKey $key2 -matchString "#AUDIT_BLOB_STORAGE_CONNECTIONSTRING" -containerName "resource-a"

#Install External Virtual Switch
New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true

#Pull docker images for Piraeus 
docker pull skunklab/orleans-silo
docker pull skunklab/tcpudpgateway
docker pull skunklab/webgateway

#Run docker compose
docker-compose up -d

#Restart the VM - enables external virtual switch
Restart-Computer
