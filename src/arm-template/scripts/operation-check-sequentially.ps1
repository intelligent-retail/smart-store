# $RESOURCE_GROUP = ""
# $PREFIX=""

$hubName = "${PREFIX}-box-service"
$boxServiceFunctionName = "${PREFIX}-box-api"
$boxServiceApiBaseUrl = "https://${boxServiceFunctionName}.azurewebsites.net/api/v1/"

$deviceId = "SmartBox1_AI"
$boxId = "SmartBox1"
$clientDeviceId = "<Past deviceId>"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$d2cMessage = @{
  box_id=$boxId
  DataTimestamp=$timestamp
  message_type="stock_start"
  items=@(
    @{
      skuCode="fudepen"
      quantity=1
    }
  )
}

# Get function key
$subscriptionId = az account show --query id --output tsv
$resourceId = "/subscriptions/$subscriptionId/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.Web/sites/${boxServiceFunctionName}"
$hostKeyUri = "https://management.azure.com${resourceId}/host/default/listkeys?api-version=2019-08-01"

$hostKey = az rest --method post --uri $hostKeyUri --query functionKeys.default --output tsv

# Create new cart
$headers = @{
  "x-functions-key" = $hostKey
  "accept" = "application/json"
}

$body = @{
  boxId = $boxId
  deviceId = $clientDeviceId
}

$cart = Invoke-RestMethod -Method post `
  -ContentType "application/json" `
  -Headers $headers `
  -Uri "${boxServiceApiBaseUrl}/carts" `
  -Body ($body | ConvertTo-Json)

# Start shopping
$d2cMessageStart = $d2cMessage.PSObject.Copy()
$d2cMessageStart.DataTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$d2cMessageStart.message_type = "stock_start"
$d2cMessageStart.items[0].quantity = 10

az iot device send-d2c-message `
  --hub-name $hubName `
  --device-id $deviceId `
  --props '$.ct=application/json;$.ce=utf-8' `
  --data (($d2cMessageStart | ConvertTo-Json -Compress) -replace '"', '""')

# Update trading diff
$d2cMessageUpdate = $d2cMessage.PSObject.Copy()
$d2cMessageUpdate.DataTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$d2cMessageUpdate.message_type = "stock_diff_trading"
$d2cMessageUpdate.items[0].quantity = -1

az iot device send-d2c-message `
  --hub-name $hubName `
  --device-id $deviceId `
  --props '$.ct=application/json;$.ce=utf-8' `
  --data (($d2cMessageUpdate | ConvertTo-Json -Compress) -replace '"', '""')

# Get items in the cart
$items = Invoke-RestMethod -Method get `
  -ContentType "application/json" `
  -Headers $headers `
  -Uri ("${boxServiceApiBaseUrl}/carts/" + $cart.cartId + "/items")

# $items | ConvertTo-Json -Depth 3

# Close cart (Close door)
$d2cMessageClose = $d2cMessage.PSObject.Copy()
$d2cMessageClose.DataTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$d2cMessageClose.message_type = "stock_end"
$d2cMessageClose.items[0].quantity = 0

az iot device send-d2c-message `
  --hub-name $hubName `
  --device-id $deviceId `
  --props '$.ct=application/json;$.ce=utf-8' `
  --data (($d2cMessageClose | ConvertTo-Json -Compress) -replace '"', '""')

# Get bill
$bill = Invoke-RestMethod -Method get `
  -ContentType "application/json" `
  -Headers $headers `
  -Uri ("${boxServiceApiBaseUrl}/carts/" + $cart.cartId + "/bill")

# $bill | ConvertTo-Json -Depth 3

# Initialize
Invoke-RestMethod -Method post `
  -ContentType "application/json" `
  -Headers $headers `
  -Uri "${boxServiceApiBaseUrl}/devices/${boxId}/status/reset"

