Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe
New-VMSwitch -name ExternalSwitch  -NetAdapterName "Ethernet 3" -AllowManagementOS $true

