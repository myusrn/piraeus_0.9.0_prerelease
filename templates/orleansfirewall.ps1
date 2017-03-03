New-NetFirewallRule -Name "Orleans Gateway" -DisplayName "Orleans Gateway" -Group Piraeus -Enabled True -Direction Inbound -Protocol TCP -RemotePort 11111
iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex
