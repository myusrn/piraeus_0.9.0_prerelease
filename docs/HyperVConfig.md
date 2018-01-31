## Hyper-V Configuration

###Step 1: Install Hyper-V

*If Hyper-V is already installed you may skip Step 1.*

Hyper-V is required to run Piraeus on Windows Containers in Docker.  If you are not running Hyper-V, you  can install it be opening Powershell as Administrator and running the following Powershell command.

`Enable-WindowsOptionalFeature -Online -FeatureName:Microsoft-Hyper-V -All`

###Step 2: Add an External Virtual Switch

Docker containers run internally on a node or VM and have internal IP addresses assigned by Docker.  Hyper-V (hypervisor) hosts these Docker containers, which means we need a method of communicating with the Hypervisor and resolving the internal Docker IP addresses.  We do this by adding an *External Virtual Switch*  to Hyper-V, which docker will automatically resolve to its internal NAT network.

Open Powershell as Administrator and check to see if you already have an External Virtual Switch by running the following Powershell command

`Get-VMSwitch`

The image below shows a result with an existing External Virtual Switch

![Check External Virtual Switch](~/images/GetVMSwitch.png "Check External Virtual Switch")

If the External Virtual Switch does not exist (likely), then follow this [video](~/videos/ExternalVirtualSwitch.mp4 "ExternalVirtualSwitch.mp4") to install.





