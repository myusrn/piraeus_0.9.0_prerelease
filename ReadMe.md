Welcome to the Skunk Lab
================

![Skunk](https://pegasusmission.files.wordpress.com/2018/02/skunklab-171x199.jpg)

-----



Overview
================

Piraeus was designed to address a futuristic vision where complex adaptive systems could communicate in real-time with simplicity.  Piraeus simplifies how heterogenous subsystems can interact dynamically and organically using an open systems approach to real-time communications. 
The technology is linearly scalable, high throughput, economical, and extreme low latency and utilizes Microsoft Orleans to facilate on-the-fly information pathways and enable communications with simplicity and without coupling.

We have used the technology with extreme experiments with great success through our work at The Pegasus Mission.  Demonstrating bidirectional and real-time communications with complex systems, distributed intelligence, and thousands of simultaneous and geographically dispersed users.  


Getting Started - Deploying Piraeus in Azure
-----------

We have automated the task of deploying Piraeus in Azure such that you can run the samples with a quick and simple deployment process. This brief [video](https://skunklabio.files.wordpress.com/2018/02/armtemplatedeploy.mp4) describes
the minimal steps necessary to deploy Piraeus in Azure.

**Deployment**

The automated deployment is performed by using an Azure Resource Manager template.  The template file "azure-deploy-template.json"
is located in the /scripts folder of the source.  You will load the custom template through the Template deployment feature 
in the Azure portal. You will need to supply the following 3 items in the template to create the deployment.

- Resource Group Name
- Storage Account Name (needed for the sample)
- Password to the Virtual Machine

You will not need to login the virtual machine as the network and VM will be configured and running
Piraeus in Azure.  After you finish the template and begin the deployment, it will take approx. 15-22 mintues to complete.

You can then follow:  [Quick Start Client Sample] (https://github.com/skunklab/piraeus_0.9.0_prerelease/blob/master/quickstartclientsample.md) to run the client samples.


**Build From Source**
- Navigate to the /build folder in source.  
- Open Visual Studio 2017 command prompt.
- Type build



Previous Work with Piraeus
----------

- [Pegasus Mission Annual Report 2016](https://pegasusmission.com/2016/12/)
- Pegasus II - IoT on the Edge of Space [articles](https://pegasusmission.com/2016/05/02/pegasus-ii-news-articles/)
- Pegasus II - IoT on the Edge of Space [video](https://www.youtube.com/watch?v=S0hP_8CeM2A) 
- NAE - Chasing the world land speed record [articles](https://pegasusmission.com/2018/01/17/pegasus-nae-articles-2016/)
- NAE - Chasing the world land speed record [video](https://www.youtube.com/watch?v=ZIlXDsdlIko)


