#Login-AzureRmAccount -AccountId Microsoft
#$pwd = Read-Host -Prompt "Enter password" -AsSecureString 
#New-AzureRmADApplication -DisplayName "DOCKER" -IdentifierUris http://docker -HomePage http://docker -Password $pwd


#DisplayName             : DOCKER
#ObjectId                : e98df3bb-6767-42cb-be8b-313125926644
#IdentifierUris          : {http://docker}
#HomePage                : http://docker
#Type                    : Application
#ApplicationId           : e0a18796-9bf8-46f5-ad31-4254ad4b6a28
#AvailableToOtherTenants : False
#AppPermissions          : 
#ReplyUrls               : {}




#Get-AzureRmADApplication -IdentifierUri http://docker

#New-AzureRmADServicePrincipal -ApplicationId "e45f1f8f-150a-4432-b2db-283a11fe22a7"


New-AzureRmRoleAssignment -RoleDefinitionName Owner -ServicePrincipalName "e0a18796-9bf8-46f5-ad31-4254ad4b6a28"

#$mySubscription = Get-AzureRmSubscription



#Get-AzureRmRoleAssignment -ServicePrincipalName "http://skunk"

#ServicePrincipalNames : {388af4c2-3afb-4f0c-ba9c-13854c30f6cd, http://skunk}
#ApplicationId         : 388af4c2-3afb-4f0c-ba9c-13854c30f6cd
#DisplayName           : SKUNK
#Id                    : e367780f-df8e-4622-b890-a5b97dd3632e
#Type                  : ServicePrincipal