Quick Start Running Client Sample
===========

Please see [Quick Start Deploying Piraeus in Azure] (quickstartazure.md) prior to performing this quick start.

---------------
**Tasks**
- Configure Piraeus with Powershell
- Run the Sample Clients


**Task 1: Configure Piraeus with Powershell**  
This brief [video](https://skunklabio.files.wordpress.com/2018/02/sampleconfigscripts.mp4) will guide you through the steps needed to configure the samples.
Following the steps below.

- [ ] Configure samples with Powershell 
- [ ] Add Azure blob storage receiver via Powershell.

**Task 2: Run the Sample Clients**  
There are 3 sample clients, which are console applications located in the /src/Samples folder.  Each client takes a "role", i.e. A or B,
which implies that role "A" can only send to "resource-a" and only receive from "resource-b".  Converse is true for a client in role "B".
There are 3 protocol clients 
- [ ] CoAP client /src/Samples/Clients/Samples.Clients.Coap/bin/Release/Samples.Clients.Coap.exe
- [ ] MQTT client /src/Samples/Clients/Samples.Clients.Mqtt/bin/Release/Samples.Clients.Mqtt.exe
- [ ] REST client /src/Samples/Clients/Samples.Clients.Rest/bin/Release/Samples.Clients.Rest.exe

You can check the blob storage storage and see the messages sent from any client in role "A".