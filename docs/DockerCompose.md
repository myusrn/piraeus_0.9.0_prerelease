##Install Docker Compose

Step 1:

Open Powershell as Administrator and run the following command to install Docker Compose.  

*Note*: If you have installed Docker for Windows at the following [link](https://docs.docker.com/docker-for-windows/install/#download-docker-for-windows), then Docker Compose is already installed.

`Invoke-WebRequest "https://github.com/docker/compose/releases/download/1.18.0/docker-compose-Windows-x86_64.exe" -UseBasicParsing -OutFile $Env:ProgramFiles\docker\docker-compose.exe`