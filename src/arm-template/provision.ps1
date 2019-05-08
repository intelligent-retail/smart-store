# item-service と stock-service の api key を pos-api に設定する
az functionapp config appsettings set `
    --resource-group ${RESOURCE_GROUP} `
    --name ${PREFIX}-pos-api `
    --settings `
        ItemMasterApiKey=${ITEM_MASTER_API_KEY} `
        StockApiKey=${STOCK_COMMAND_API_KEY}

# # pos-service の api key と通知の設定を box-api に設定する
# az functionapp config appsettings set `
#     --resource-group ${RESOURCE_GROUP} `
#     --name ${BOX_FUNCTIONS_NAME} `
#     --settings `
#         NotificationApiKey=${NOTIFICATION_API_KEY} `
#         NotificationUri=${NOTIFICATION_URI} `
#         PosApiKey=${POS_API_KEY}

# Get a server name of SQL Server
$SQL_SERVER_NAME=az sql server list `
    --resource-group ${RESOURCE_GROUP} `
    --query "[0].name" `
    --output tsv

# Set a temporary firewall rule to enable to access from your machine
$IP_ADDRESS=Invoke-RestMethod 4.ipaddr.io
az sql server firewall-rule create `
    --resource-group ${RESOURCE_GROUP} `
    --server ${SQL_SERVER_NAME} `
    --name temp `
    --start-ip-address ${IP_ADDRESS} `
    --end-ip-address ${IP_ADDRESS}

# Create table in SQL Database
$SQL_SERVER_ENDPOINT=az sql server list `
    --resource-group ${RESOURCE_GROUP} `
    --query "[0].fullyQualifiedDomainName" `
    --output tsv
$SQL_SERVER_ADMIN_USER=az sql server list `
    --resource-group ${RESOURCE_GROUP} `
    --query "[0].administratorLogin" `
    --output tsv
$SQL_DATABASE_NAME=az sql db list `
    --resource-group ${RESOURCE_GROUP} `
    --server ${SQL_SERVER_NAME} `
    --query "[1].name" `
    --output tsv

sqlcmd -S ${SQL_SERVER_ENDPOINT} `
    -U ${SQL_SERVER_ADMIN_USER} `
    -P "${STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}" `
    -d ${SQL_DATABASE_NAME} `
    -i src/arm-template/createStocksInStockBackend.sql

# Remove the temporary firewall rule
az sql server firewall-rule delete `
    --resource-group ${RESOURCE_GROUP} `
    --server ${SQL_SERVER_NAME} `
    --name temp

# Set IoT Hub
$IOT_HUB_NAME=az iot hub list `
  --resource-group ${RESOURCE_GROUP} `
  --query '[0].name' `
  --output tsv

az extension add --name azure-cli-iot-ext

az iot hub device-identity create `
  --resource-group ${RESOURCE_GROUP} `
  --hub-name ${IOT_HUB_NAME} `
  --device-id SmartBox1_AI
az iot hub device-identity create `
  --resource-group ${RESOURCE_GROUP} `
  --hub-name ${IOT_HUB_NAME} `
  --device-id SmartBox1_Device
az iot hub device-identity create `
  --resource-group $RESOURCE_GROUP `
  --hub-name $IOT_HUB_NAME `
  --device-id SmartBox2_AI
az iot hub device-identity create `
  --resource-group $RESOURCE_GROUP `
  --hub-name $IOT_HUB_NAME `
  --device-id SmartBox2_Device

$IOT_CONN_STR=az iot hub show-connection-string `
  --resource-group ${RESOURCE_GROUP} `
  --name ${IOT_HUB_NAME} `
  --output tsv
$EVENT_HUB_ENDPOINT=az iot hub show `
  --query properties.eventHubEndpoints.events.endpoint `
  --name ${IOT_HUB_NAME} `
  --output tsv
$ENTITY_PATH=az iot hub show `
  --query properties.eventHubEndpoints.events.path `
  --name ${IOT_HUB_NAME} `
  --output tsv
$SHARED_ACCESS_KEY=az iot hub policy show `
  --resource-group ${RESOURCE_GROUP} `
  --name iothubowner `
  --hub-name ${IOT_HUB_NAME} `
  --query primaryKey `
  --output tsv
$IOT_EVENT_HUB_CONN_STR="Endpoint=${EVENT_HUB_ENDPOINT};SharedAccessKeyName=iothubowner;SharedAccessKey=${SHARED_ACCESS_KEY};EntityPath=${ENTITY_PATH}"

# Set functions settings
$BOX_FUNCTIONS_NAME="${PREFIX}-box-api"
az webapp config appsettings set `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_FUNCTIONS_NAME} `
  --settings `
    NotificationApiKey=${NOTIFICATION_API_KEY} `
    NotificationUri=${NOTIFICATION_URI} `
    PosApiKey=${POS_API_KEY} `
    IotHubConnectionString=${IOT_CONN_STR} `
    IotHubEventConnectionString=${IOT_EVENT_HUB_CONN_STR}

# Create collections of Cosmos DB for item-service
$ITEM_SERVICE_COSMOSDB_DATABASE="00100"
$ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT="400"
$ITEM_SERVICE_COSMOSDB_COLLECTION="Items"
$ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY="/storeCode"

$ITEM_SERVICE_COSMOSDB=az cosmosdb list `
    --resource-group ${RESOURCE_GROUP} `
    --query "[?contains(@.name, 'item')==``true``].name" `
    --output tsv
az cosmosdb database create `
    --resource-group-name ${RESOURCE_GROUP} `
    --name ${ITEM_SERVICE_COSMOSDB} `
    --db-name ${ITEM_SERVICE_COSMOSDB_DATABASE} `
    --throughput ${ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT}
az cosmosdb collection create `
    --resource-group-name ${RESOURCE_GROUP} `
    --name ${ITEM_SERVICE_COSMOSDB} `
    --db-name ${ITEM_SERVICE_COSMOSDB_DATABASE} `
    --collection-name ${ITEM_SERVICE_COSMOSDB_COLLECTION} `
    --partition-key-path ${ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY}

$ITEM_SERVICE_COSMOSDB_CONNSTR=az cosmosdb list-connection-strings `
    --resource-group ${RESOURCE_GROUP} `
    --name ${ITEM_SERVICE_COSMOSDB} `
    --query "connectionStrings[0].connectionString" `
    --output tsv

dt `
  /s:JsonFile `
  /s.Files:.\\src\\arm-template\\sample-data\\public\\item-service\\itemMasterSampleData.json `
  /t:DocumentDB `
  /t.ConnectionString:"${ITEM_SERVICE_COSMOSDB_CONNSTR};Database=${ITEM_SERVICE_COSMOSDB_DATABASE};" `
  /t.Collection:${ITEM_SERVICE_COSMOSDB_COLLECTION} `
  /t.PartitionKey:${ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY} `
  /t.CollectionThroughput:400

# Create collections of Cosmos DB for pos-service
$POS_DB_ACCOUNT_NAME="${PREFIX}-pos-service"
$POS_DB_NAME="smartretailpos"

az cosmosdb database create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --throughput 500
az cosmosdb database create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --db-name ${BOX_DB_NAME} `
  --throughput 400
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --collection-name PosMasters `
  --partition-key-path /mastername
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --collection-name Carts `
  --partition-key-path /cartId `
  --default-ttl 604800
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --collection-name TransactionLogs `
  --partition-key-path /key
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --collection-name Receipts `
  --partition-key-path /key
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --db-name ${POS_DB_NAME} `
  --collection-name Counters `
  --partition-key-path /terminalKey

$POS_SERVICE_COSMOSDB_CONNSTR=az cosmosdb list-connection-strings `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --query "connectionStrings[0].connectionString" `
  --output tsv

dt `
  /s:JsonFile `
  /s.Files:.\\src\\arm-template\\sample-data\\public\\pos-service\\PosMasters.json `
  /t:DocumentDB `
  /t.ConnectionString:"${POS_SERVICE_COSMOSDB_CONNSTR};Database=${POS_DB_NAME};" `
  /t.Collection:PosMasters

# Create collections of Cosmos DB for box-service
$BOX_DB_ACCOUNT_NAME="${PREFIX}-box-service"
$BOX_DB_NAME="smartretailboxmanagement"

az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --db-name ${BOX_DB_NAME} `
  --collection-name BoxManagements `
  --partition-key-path /boxId
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --db-name ${BOX_DB_NAME} `
  --collection-name Terminals `
  --partition-key-path /boxId
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --db-name ${BOX_DB_NAME} `
  --collection-name Skus `
  --partition-key-path /companyCode
az cosmosdb collection create `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --db-name ${BOX_DB_NAME} `
  --collection-name Stocks `
  --partition-key-path /boxId

$BOX_SERVICE_COSMOSDB_CONNSTR=az cosmosdb list-connection-strings `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --query "connectionStrings[0].connectionString" `
  --output tsv

dt `
  /s:JsonFile `
  /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Skus.json `
  /t:DocumentDB `
  /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" `
  /t.Collection:Skus
dt `
  /s:JsonFile `
  /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Terminals.json `
  /t:DocumentDB `
  /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" `
  /t.Collection:Terminals
