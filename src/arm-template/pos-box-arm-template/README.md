# Deploy resources with ARM template

## リソースのデプロイ

Azure へリソースをデプロイします。

Azure CLI を利用しますので、下記を参考に環境をご準備ください。

- [Azure CLI](https://docs.microsoft.com/ja-jp/cli/azure)

Azure CLI が準備できましたら、下記を参考にリソースをデプロイしてください。

```bash
RESOURCE_GROUP=<resource group name>
LOCATION=japaneast

PREFIX=<prefix string within 2 characters>
ITEM_MASTER_API_KEY=<item service api key>
ITEM_MASTER_URI=https://<item service name>.azurewebsites.net/api/v1/company/{company-code}/store/{store-code}/items
STOCK_API_KEY=<stock service api key>
STOCK_URI=https://<stock service name>.azurewebsites.net/api/v1/stocks
NOTIFICATION_API_KEY=<app center push api key>
NOTIFICATION_URI=https://api.appcenter.ms/v0.1/apps/<app center push name>/SmartRetailApp.Android/push/notifications
POS_API_KEY=<pos api key>

# リソースグループを作成する
az group create \
    --name ${RESOURCE_GROUP} \
    --location ${LOCATION}

# 作成したリソースグループの中に、リソースをデプロイする
az group deployment create \
    --resource-group ${RESOURCE_GROUP} \
    --template-file src/arm-template/pos-box-arm-template/template.json \
    --parameters @src/arm-template/pos-box-arm-template/parameters.json \
    --parameters \
        prefix=${PREFIX} \
        itemMasterApiKey=${ITEM_MASTER_API_KEY} \
        itemMasterUri=${ITEM_MASTER_URI} \
        stockApiKey=${STOCK_API_KEY} \
        stockUri=${STOCK_URI} \
        notificationApiKey=${NOTIFICATION_API_KEY} \
        notificationUri=${NOTIFICATION_URI} \
        posApiKey=${POS_API_KEY}
```

## プロビジョニング

リソースのデプロイができましたら、下記のプロビジョニングおよびデータ登録を行います。

- IoT Hub の IoT デバイスの登録
- IoT Hub とBOX管理サービスの紐づけ
- Box管理サービス・POSサービスのマスタの準備

### IoT Hub の IoT デバイスの登録

Azure CLI にて 以下を参考に IoT Hub の IoT デバイスを作成します。

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
IOT_HUB_NAME=<iot hub name>

# Install the Azure CLI extension for device identity
az extension add --name azure-cli-iot-ext

# Create device identity
az iot hub device-identity create --resource-group $RESOURCE_GROUP \
    --hub-name $IOT_HUB_NAME --device-id SmartBox1_AI
az iot hub device-identity create --resource-group $RESOURCE_GROUP \
    --hub-name $IOT_HUB_NAME --device-id SmartBox1_Device
az iot hub device-identity create --resource-group $RESOURCE_GROUP \
    --hub-name $IOT_HUB_NAME --device-id SmartBox2_AI
az iot hub device-identity create --resource-group $RESOURCE_GROUP \
    --hub-name $IOT_HUB_NAME --device-id SmartBox2_Device
```

## IoT Hub と Box管理サービスの紐づけ

Azure CLI にて以下を参考にIoT Hub と Box管理サービスの紐づけを行います。

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
IOT_HUB_NAME=<iot hub name>
BOX_FUNCTIONS_NAME=<box service name>

# Create parameters
IOT_CONN_STR=$(az iot hub show-connection-string --resource-group ${RESOURCE_GROUP} --hub-name ${IOT_HUB_NAME} --output tsv)
EVENT_HUB_ENDPOINT=$(az iot hub show --query properties.eventHubEndpoints.events.endpoint --name ${IOT_HUB_NAME} --output tsv)
ENTITY_PATH=$(az iot hub show --query properties.eventHubEndpoints.events.path --name ${IOT_HUB_NAME} --output tsv)
SHARED_ACCESS_KEY=$(az iot hub policy show --resource-group ${RESOURCE_GROUP} --name iothubowner --hub-name ${IOT_HUB_NAME} --query primaryKey --output tsv)
IOT_EVENT_HUB_CONN_STR="Endpoint=${EVENT_HUB_ENDPOINT};SharedAccessKeyName=iothubowner;SharedAccessKey=${SHARED_ACCESS_KEY};EntityPath=${ENTITY_PATH}"

# Set functions settings
az webapp config appsettings set --resource-group ${RESOURCE_GROUP} --name ${BOX_FUNCTIONS_NAME} --settings IotHubConnectionString=${IOT_CONN_STR}
az webapp config appsettings set --resource-group ${RESOURCE_GROUP} --name ${BOX_FUNCTIONS_NAME} --settings IotHubEventConnectionString=${IOT_EVENT_HUB_CONN_STR}
```

### Box管理サービス・POSサービスのマスタの準備

Box管理サービス・POSサービスのマスタを Azure Cosmos DB に準備し、必要に応じてデータのを登録します。
まず、 Azure Cosmos DB (SQL API) にコレクションを作成します。

```bash

# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
POS_DB_ACCOUNT_NAME=<cosmos db used for pos service name>
BOX_DB_ACCOUNT_NAME=<cosmos db used for box management service name>

# Create cosmosdb database and collection
POS_DB_NAME='smartretailpos'
BOX_DB_NAME='smartretailboxmanagement'

az cosmosdb database create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME --throughput 500
az cosmosdb database create --resource-group $RESOURCE_GROUP \
    --name $BOX_DB_ACCOUNT_NAME --db-name $BOX_DB_NAME --throughput 400
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME \
    --collection-name PosMasters \
    --partition-key-path /mastername
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME \
    --collection-name Carts \
    --partition-key-path /cartId \
    --default-ttl 604800
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME \
    --collection-name TransactionLogs \
    --partition-key-path /key
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME \
    --collection-name Receipts \
    --partition-key-path /key
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $POS_DB_ACCOUNT_NAME --db-name $POS_DB_NAME \
    --collection-name Counters \
    --partition-key-path /terminalKey
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $BOX_DB_ACCOUNT_NAME --db-name $BOX_DB_NAME \
    --collection-name BoxManagements \
    --partition-key-path /boxId
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $BOX_DB_ACCOUNT_NAME --db-name $BOX_DB_NAME \
    --collection-name Terminals \
    --partition-key-path /boxId
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $BOX_DB_ACCOUNT_NAME --db-name $BOX_DB_NAME \
    --collection-name Skus \
    --partition-key-path /companyCode
az cosmosdb collection create --resource-group $RESOURCE_GROUP \
    --name $BOX_DB_ACCOUNT_NAME --db-name $BOX_DB_NAME \
    --collection-name Stocks \
    --partition-key-path /boxId
```

つぎに、データの登録を行います。  
データ移行は様々な方法が提供されています。ここでは、以下の作業を Azure CLI およびデータ移行ツール ( `dt` コマンド) を用いて、コマンドラインで実施する方法をご紹介します。インポートファイルを用意しておりますが、適宜読み替えてご参考ください。

- [データ移行ツール ( `dt` コマンド) を使用して Azure Cosmos DB にデータを移行する](https://docs.microsoft.com/ja-jp/azure/cosmos-db/import-data)

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
POS_DB_ACCOUNT_NAME=<cosmos db used for pos service name>
BOX_DB_ACCOUNT_NAME=<cosmos db used for box management service name>
POS_DB_NAME='smartretailpos'
BOX_DB_NAME='smartretailboxmanagement'

# Create connection string
POS_SERVICE_COSMOSDB_CONNSTR=$(az cosmosdb list-connection-strings \
    --resource-group ${RESOURCE_GROUP} \
    --name ${POS_DB_ACCOUNT_NAME} \
    --query "connectionStrings[0].connectionString" \
    --output tsv)
BOX_SERVICE_COSMOSDB_CONNSTR=$(az cosmosdb list-connection-strings \
    --resource-group ${RESOURCE_GROUP} \
    --name ${BOX_DB_ACCOUNT_NAME} \
    --query "connectionStrings[0].connectionString" \
    --output tsv)

# Insert documents to Cosmos DB
<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\pos-box-arm-template\\sample-data\\public\\pos-service\\PosMasters.json \
    /t:DocumentDB \
    /t.ConnectionString:"${POS_SERVICE_COSMOSDB_CONNSTR};Database=${POS_DB_NAME};" \
    /t.Collection:PosMasters
<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\pos-box-arm-template\\sample-data\\public\\box-service\\Skus.json \
    /t:DocumentDB \
    /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" \
    /t.Collection:Skus
<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\pos-box-arm-template\\sample-data\\public\\box-service\\Terminals.json \
    /t:DocumentDB \
    /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" \
    /t.Collection:Terminals
```
