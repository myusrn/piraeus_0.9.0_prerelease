Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe
docker pull skunklab/orleans-silo
docker pull skunklab/tcpudpgateway
docker pull skunklab/webgateway
New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true
Restart-Computer


