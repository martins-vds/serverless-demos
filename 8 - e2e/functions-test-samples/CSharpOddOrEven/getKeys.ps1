Param(
	[string]$appName,
	[string]$functionName,
	[string]$resourceGroup
)

$publish = Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroup -ResourceType Microsoft.Web/sites/config -ResourceName "$($appName)/publishingcredentials" -Action list -ApiVersion 2015-08-01 -Force
$username = $publish.properties.publishingUserName
$password = $publish.properties.publishingPassword
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

$scmUrl = "https://$($siteName).scm.azurewebsites.net"
$token = Invoke-RestMethod -Uri "https://$($appName).scm.azurewebsites.net/api/functions/admin/token" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method GET
$keyResponse = Invoke-RestMethod -Uri "https://$($appName).azurewebsites.net/admin/functions/$($functionName)/keys" -Headers @{Authorization=("Bearer {0}" -f $token)} -Method GET
$key = $keyResponse.keys[0].value

Write-Host "##vso[task.setvariable variable=functionUrl;isSecret=false;isOutput=true;]https://$($appName).azurewebsites.net/api/$($functionName)&code=$($key)"