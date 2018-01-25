Add-AzureIotHubCommandSubscription  -ResourceUriString "" -Host "" -DeviceId "" -KeyName "" -PropertyName "" -PropertyValue "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureIotHubDirectMethodSubscription -ResourceUriString "" -Host "" -DeviceId "" -Method "" -KeyName "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureIotHubDeviceSubscription -ResourceUriString "" -Host "" -DeviceId "" -PropertyName "" -PropertyValue "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureBlobStorageSubscription -ResourceUriString "" -Host "" -BlobType "[Append | Page | Block]" -Container "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureQueueStorageSubscription -ResourceUriString "" -Host "" -Queue "" -TTL "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureCosmosDbSubscription -ResourceUriString "" -Host "" -Database "" -Collection "" -Key "" -ServiceUrl "" -SecurityToken ""
Add-AzureWebServiceSubscription -ResourceUriString "" -WebServiceUrl "" -Issuer "" -Audience "" -TokenType "[Jwt | Swt X509 | None]" -Key "" -ServiceUrl "" -SecurityToken "" 
Add-AzureEventHubSubscription -ResourceUriString "" -Host "" -Hub "" -PartitionId "" -KeyName "" -Key "" -ServiceUrl "" -SecurityToken ""



