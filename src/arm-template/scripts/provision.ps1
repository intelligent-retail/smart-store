$RESOURCE_GROUP="<resource group name>"
$PREFIX="<prefix string within 2 characters>"
$STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD="<sql server admin password>"

# Get a server name of SQL Server
$SQL_SERVER_NAME=az sql server list `
    --resource-group ${RESOURCE_GROUP} `
    --query "[0].name" `
    --output tsv

# Set a temporary firewall rule to enable to access from your machine
$IP_ADDRESS=Invoke-RestMethod http://ifconfig.me/ip
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

# Create device identity
$IOT_HUB_NAME=az iot hub list `
  --resource-group ${RESOURCE_GROUP} `
  --query '[0].name' `
  --output tsv

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

$ITEM_SERVICE_COSMOSDB_CONNSTR=az cosmosdb keys list `
    --resource-group ${RESOURCE_GROUP} `
    --name ${ITEM_SERVICE_COSMOSDB} `
    --type connection-strings `
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

$POS_SERVICE_COSMOSDB_CONNSTR=az cosmosdb keys list `
  --resource-group ${RESOURCE_GROUP} `
  --name ${POS_DB_ACCOUNT_NAME} `
  --type connection-strings `
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

$BOX_SERVICE_COSMOSDB_CONNSTR=az cosmosdb keys list `
  --resource-group ${RESOURCE_GROUP} `
  --name ${BOX_DB_ACCOUNT_NAME} `
  --type connection-strings `
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
