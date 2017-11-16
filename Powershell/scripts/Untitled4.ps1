#import the Piraeus Powershell Module
import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Debug\Piraeus.Module.dll

#Set the URL for the Piraeus Management API and Primary to gain access
$serviceUrl = "http://localhost:4163"
$key = "12345678"

#Get the management security token for the session
$securityToken = Get-PiraeusManagementToken -ServiceUrl $serviceUrl -Key $key


#Setting up your first resource

#Create 2 access control policies using CAPL.
#First policy control access for publishers to the resource
#Second policy controls access for subscribers to the resource

$match = New-CaplMatch -Type Literal -ClaimType "http://www.skunklab.io/role" -Required $true
$operation = New-CaplOperation -Type Equal -Value 'manage'
$rule = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation
$policy1 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/pub" -EvaluationExpression $rule
$policy2 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/sub" -EvaluationExpression $rule
Set-CaplPolicy -ServiceUrl $serviceUrl -SecurityToken $securitytoken -Policy $policy1
Set-CaplPolicy -ServiceUrl $serviceUrl -SecurityToken $securitytoken -Policy $policy2
#Set-ResourceMetadata -Enabled $true -RequireEncryptedChannel $false -ResourceUriString "http://www.skunklab.io/resource1" -PublishPolicyUriString "http://www.skunklab.io/policy/resource1/pub" -SubscribePolicyUriString "http://www.skunklab.io/policy/resource1/sub" -ServiceUrl $serviceUrl -SecurityToken $securityToken



#Create metadata for the resource; this describes the resource, behaviors, and capabilities
#Remember the PublishPolicyUriString and the SubscribePolicyUriString parameters reference the CAPL policies we just created

Set-PiraeusResourceMetadata -Enabled $true -RequireEncryptedChannel $false -ResourceUriString "http://www.skunklab.io/resource1" -PublishPolicyUriString "http://www.skunklab.io/policy/resource1/pub" -SubscribePolicyUriString "http://www.skunklab.io/policy/resource1/sub" -ServiceUrl $serviceUrl -SecurityToken $securityToken

#Let's create a subscription for the resource we just created
#It's going to a subscription for a REST Web service 
$ttl = New-TimeSpan -Minutes 5

$subscriptionUriString = New-PiraeusSubscribe -ServiceUrl $serviceUrl -SecurityToken $securityToken -ResourceUriString http://www.skunklab.io/resource1 -Identity matt -TTL $ttl
Write-Output "SubscriptionUriString " 
$subscriptionUriString



$submetadata = Get-PiraeusSubscriptionMetadata -ServiceUrl $serviceUrl -SecurityToken $securityToken -SubscriptionUriString $subscriptionUriString
Write-Output "Subscription Metadata"
$submetadata

$v = Get-PiraeusResourceSubscriptionList -ServiceUrl $serviceUrl -SecurityToken $securitytoken -ResourceUriString "http://www.skunklab.io/resource1"
Write-Output "list"
$v

New-PiraeusUnsubscribe -ServiceUrl $serviceUrl -SecurityToken $securityToken -SubscriptionUriString $subscriptionUriString
Write-Output "Unsubscribed"



$listOfResources = Get-PiraeusResourceList -ServiceUrl $serviceUrl -SecurityToken $securityToken
$listOfResources

$metadata = Get-PiraeusResourceMetadata -ServiceUrl $serviceUrl -SecurityToken $securityToken -ResourceUriString http://www.skunklab.io/resource1
$metadata


#$uriStrig = Set-PiraeusSubscriptionMetadata -ServiceUrl $serviceUrl -SecurityToken $securityToken -Identity matt 
#$uriString 

#$metadata = Get-PiraeusResourceMetadata -ServiceUrl $serviceUrl -SecurityToken $securityToken -ResourceUriString http://www.skunklab.io/resource1



