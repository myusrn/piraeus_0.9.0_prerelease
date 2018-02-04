Quick Start Deploying Piraeus in Azure
===========


---------------
We have automated the task of deploying Piraeus in Azure such that you can 
run the samples with a quick and simple deployment process. The following [video](https://skunklabio.files.wordpress.com/2018/02/armtemplatedeploy.mp4) describes
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

You can then go to the [Quick Start Running Client Sample] (https://github.com/skunklab/piraeus_0.9.0_prerelease/blob/master/quickstartclientsample.md) for running the client samples.

