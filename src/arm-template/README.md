# Deploy resources with ARM template

## リソースのデプロイ

Azure へリソースをデプロイします。

Azure CLI を利用しますので、下記を参考に環境をご準備ください。

- [Azure CLI](https://docs.microsoft.com/ja-jp/cli/azure)

Azure CLI が準備できましたら、下記を参考にリソースをデプロイしてください。

### PowerShell の場合

```ps1
$RESOURCE_GROUP="<resource group name>"
$LOCATION="japaneast"

$PREFIX="<prefix string within 2 characters>"
$STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD="<sql server admin password>"

$TEMPLATE_URL="https://raw.githubusercontent.com/intelligent-retail/smart-store/master/src/arm-template"

# リソースグループを作成する
az group create `
  --name ${RESOURCE_GROUP} `
  --location ${LOCATION}

# 作成したリソースグループの中に、リソースをデプロイする
az group deployment create `
  --resource-group ${RESOURCE_GROUP} `
  --template-uri ${TEMPLATE_URL}/template.json `
  --parameters ${TEMPLATE_URL}/parameters.json `
  --parameters `
    prefix=${PREFIX} `
    stockServiceSqlServerAdminPassword=${STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}

# item-service と stock-service の api key を pos-api に設定する
$ITEM_MASTER_API_KEY="<item service api key>"
$STOCK_COMMAND_API_KEY="<stock service command api key>"
az functionapp config appsettings set `
  --resource-group ${RESOURCE_GROUP} `
  --name ${PREFIX}-pos-api `
  --settings `
    ItemMasterApiKey=${ITEM_MASTER_API_KEY} `
    StockApiKey=${STOCK_COMMAND_API_KEY}

# pos-service の api key と通知の設定を box-api に設定する
$POS_API_KEY="<pos api key>"
$NOTIFICATION_API_KEY="<app center push api key>"
$NOTIFICATION_URI="https://api.appcenter.ms/v0.1/apps/<app center push name>/SmartRetailApp.Android/push/notifications"
az functionapp config appsettings set `
  --resource-group ${RESOURCE_GROUP} `
  --name ${PREFIX}-box-api `
  --settings `
    NotificationApiKey=${NOTIFICATION_API_KEY} `
    NotificationUri=${NOTIFICATION_URI} `
    PosApiKey=${POS_API_KEY}
```

### bash の場合

```bash
RESOURCE_GROUP=<resource group name>
LOCATION=japaneast

PREFIX=<prefix string within 2 characters>
STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD=<sql server admin password>

TEMPLATE_URL=https://raw.githubusercontent.com/intelligent-retail/smart-store/master/src/arm-template

# リソースグループを作成する
az group create \
  --name ${RESOURCE_GROUP} \
  --location ${LOCATION}

# 作成したリソースグループの中に、リソースをデプロイする
az group deployment create \
  --resource-group ${RESOURCE_GROUP} \
  --template-uri ${TEMPLATE_URL}/template.json \
  --parameters ${TEMPLATE_URL}/parameters.json \
  --parameters \
    prefix=${PREFIX} \
    stockServiceSqlServerAdminPassword=${STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}

# item-service と stock-service の api key を pos-api に設定する
ITEM_MASTER_API_KEY=<item service api key>
STOCK_COMMAND_API_KEY=<stock service command api key>
az functionapp config appsettings set \
  --resource-group ${RESOURCE_GROUP} \
  --name ${PREFIX}-pos-api \
  --settings \
    ItemMasterApiKey=${ITEM_MASTER_API_KEY} \
    StockApiKey=${STOCK_COMMAND_API_KEY}

# pos-service の api key と通知の設定を box-api に設定する
POS_API_KEY=<pos api key>
NOTIFICATION_API_KEY=<app center push api key>
NOTIFICATION_URI=https://api.appcenter.ms/v0.1/apps/<app center push name>/SmartRetailApp.Android/push/notifications
az functionapp config appsettings set \
  --resource-group ${RESOURCE_GROUP} \
  --name ${PREFIX}-box-api \
  --settings \
    NotificationApiKey=${NOTIFICATION_API_KEY} \
    NotificationUri=${NOTIFICATION_URI} \
    PosApiKey=${POS_API_KEY}
```

## プロビジョニング

リソースのデプロイができましたら、下記のプロビジョニングおよびデータ登録を行います。

- SQLデータベースのテーブル作成
- IoT Hub の IoT デバイスの登録
- IoT Hub とBOX管理サービスの紐づけ
- 各種マスタの準備
  - 商品マスタの準備
    - Cosmos DB のコレクション作成
    - 画像データのアップロード、およびデータのインポート
  - Box管理サービス・POSサービスのマスタの準備

### SQLデータベースのテーブル作成

Azure ポータルのクエリエディタや `sqlcmd` などを利用してテーブルを作成します。テーブル作成に利用するクエリは [createStocksInStockBackend.sql](./createStocksInStockBackend.sql) をご参考ください。

#### Azure ポータルのクエリエディタを使用する場合

クエリエディタを使用する場合は、こちらをご参考ください。

- [Azure portal:クエリ エディターを使用した Azure SQL Database の照会 | Microsoft Docs](https://docs.microsoft.com/ja-jp/azure/sql-database/sql-database-connect-query-portal)

#### `sqlcmd` コマンドを使用する場合

`sqlcmd` を使用する場合は、下記をご参考ください。

- Windows: [sqlcmd ユーティリティ](https://docs.microsoft.com/ja-jp/sql/tools/sqlcmd-utility?view=sql-server-2017)
- Linux: [sqlcmd および bcp、SQL Server コマンド ライン ツールを Linux にインストールする](https://docs.microsoft.com/ja-jp/sql/linux/sql-server-linux-setup-tools?view=sql-server-2017)

```bash
RESOURCE_GROUP=<resource group name>

# Get a server name of SQL Server
SQL_SERVER_NAME=$(az sql server list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[0].name" \
    --output tsv)

# Set a temporary firewall rule to enable to access from your machine
IP_ADDRESS=$(curl 4.ipaddr.io)
az sql server firewall-rule create \
    --resource-group ${RESOURCE_GROUP} \
    --server ${SQL_SERVER_NAME} \
    --name temp \
    --start-ip-address ${IP_ADDRESS} \
    --end-ip-address ${IP_ADDRESS}

# Create table in SQL Database
SQL_SERVER_ENDPOINT=$(az sql server list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[0].fullyQualifiedDomainName" \
    --output tsv)
SQL_SERVER_ADMIN_USER=$(az sql server list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[0].administratorLogin" \
    --output tsv)
SQL_DATABASE_NAME=$(az sql db list \
    --resource-group ${RESOURCE_GROUP} \
    --server ${SQL_SERVER_NAME} \
    --query "[1].name" \
    --output tsv)

sqlcmd -S ${SQL_SERVER_ENDPOINT} \
    -U ${SQL_SERVER_ADMIN_USER} \
    -P "${STOCK_SERVICE_SQL_SERVER_ADMIN_PASSWORD}" \
    -d ${SQL_DATABASE_NAME} \
    -i src/arm-template/createStocksInStockBackend.sql

# Remove the temporary firewall rule
az sql server firewall-rule delete \
    --resource-group ${RESOURCE_GROUP} \
    --server ${SQL_SERVER_NAME} \
    --name temp
```

### IoT Hub の IoT デバイスの登録

Azure CLI にて 以下を参考に IoT Hub の IoT デバイスを作成します。

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
IOT_HUB_NAME=$(az iot hub list --resource-group ${RESOURCE_GROUP} --query '[0].name' --output tsv)

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

### IoT Hub と Box管理サービスの紐づけ

Azure CLI にて以下を参考にIoT Hub と Box管理サービスの紐づけを行います。

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
IOT_HUB_NAME=$(az iot hub list --resource-group ${RESOURCE_GROUP} --query '[0].name' --output tsv)
BOX_FUNCTIONS_NAME=${PREFIX}-box-api

# Create parameters
IOT_CONN_STR=$(az iot hub show-connection-string --resource-group ${RESOURCE_GROUP} --name ${IOT_HUB_NAME} --output tsv)
EVENT_HUB_ENDPOINT=$(az iot hub show --query properties.eventHubEndpoints.events.endpoint --name ${IOT_HUB_NAME} --output tsv)
ENTITY_PATH=$(az iot hub show --query properties.eventHubEndpoints.events.path --name ${IOT_HUB_NAME} --output tsv)
SHARED_ACCESS_KEY=$(az iot hub policy show --resource-group ${RESOURCE_GROUP} --name iothubowner --hub-name ${IOT_HUB_NAME} --query primaryKey --output tsv)
IOT_EVENT_HUB_CONN_STR="Endpoint=${EVENT_HUB_ENDPOINT};SharedAccessKeyName=iothubowner;SharedAccessKey=${SHARED_ACCESS_KEY};EntityPath=${ENTITY_PATH}"

# Set functions settings
az webapp config appsettings set --resource-group ${RESOURCE_GROUP} --name ${BOX_FUNCTIONS_NAME} --settings IotHubConnectionString=${IOT_CONN_STR}
az webapp config appsettings set --resource-group ${RESOURCE_GROUP} --name ${BOX_FUNCTIONS_NAME} --settings IotHubEventConnectionString=${IOT_EVENT_HUB_CONN_STR}
```

### 各種マスタの準備

#### 商品マスタの準備

商品マスタの Azure Cosmos DB の準備と、必要に応じてデータのを登録します。

まず、 Azure Cosmos DB (SQL API) にコレクションを作成します。

```bash
RESOURCE_GROUP=<resource group name>

# Create collections of Cosmos DB
ITEM_SERVICE_COSMOSDB_DATABASE=00100
ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT=400
ITEM_SERVICE_COSMOSDB_COLLECTION=Items
ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY=/storeCode

ITEM_SERVICE_COSMOSDB=$(az cosmosdb list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[?contains(@.name, 'item')==\`true\`].name" \
    --output tsv)
az cosmosdb database create \
    --resource-group-name ${RESOURCE_GROUP} \
    --name ${ITEM_SERVICE_COSMOSDB} \
    --db-name ${ITEM_SERVICE_COSMOSDB_DATABASE} \
    --throughput ${ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT}
az cosmosdb collection create \
    --resource-group-name ${RESOURCE_GROUP} \
    --name ${ITEM_SERVICE_COSMOSDB} \
    --db-name ${ITEM_SERVICE_COSMOSDB_DATABASE} \
    --collection-name ${ITEM_SERVICE_COSMOSDB_COLLECTION} \
    --partition-key-path ${ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY}
```

つぎに、必要に応じて、画像のアップロードおよびデータの登録を行います。

画像のアップロードや Cosmos DB の操作は様々な方法が提供されていますので、適宜ご利用ください。

- Azure Blob Storage へのアップロード
  - [Azure portal を使用して BLOB をアップロード、ダウンロード、および一覧表示する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-portal)
  - [Azure Storage Explorer を使用してオブジェクト ストレージ内に BLOB を作成する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-storage-explorer)
  - [Azure CLI を使用して BLOB をアップロード、ダウンロード、および一覧表示する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-cli)
- Azure Cosmos DB へのインポート
  - [(ポータルを用いた) サンプル データの追加](https://docs.microsoft.com/ja-jp/azure/cosmos-db/create-sql-api-dotnet#add-sample-data)
  - [データ移行ツール ( `dt` コマンド) を使用して Azure Cosmos DB にデータを移行する](https://docs.microsoft.com/ja-jp/azure/cosmos-db/import-data)
  - [Azure Cosmos DB Bulk Executor ライブラリの概要](https://docs.microsoft.com/ja-jp/azure/cosmos-db/bulk-executor-overview)

ここでは、以下の作業を Azure CLI およびデータ移行ツール ( `dt` コマンド) を用いて、コマンドラインで実施する方法をご紹介します。サンプルの画像とインポートファイルを用意しておりますが、適宜読み替えてご参考ください。

1. 上記手順を参考に、変数を設定する
   - `RESOURCE_GROUP`
   - `ITEM_SERVICE_COSMOSDB_DATABASE`
   - `ITEM_SERVICE_COSMOSDB_COLLECTION`
   - `ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY`
1. `sample-data/public/item-service/images` ディレクトリ配下に格納されている png 画像を Azure Blog Storage にアップロードする
1. アップロードした URL をインポート用の JSON データに反映する
1. JSON データを Cosmos DB のコレクションにインポートする

```bash
# Set variables following above, if you did not set them

# Upload assets images
ASSETS_BLOB_STORAGE_NAME=$(az storage account list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[?contains(@.name, 'assets')==\`true\`].name" \
    --output tsv)
ASSETS_BLOB_STORAGE_CONTAINER=$(az storage container list \
    --account-name ${ASSETS_BLOB_STORAGE_NAME} \
    --query "[0].name" \
    --output tsv)
ASSETS_BLOB_STORAGE_CONNSTR=$(az storage account show-connection-string \
    --name ${ASSETS_BLOB_STORAGE_NAME} \
    --query "connectionString" \
    --output tsv)

az storage blob upload-batch \
    --connection-string ${ASSETS_BLOB_STORAGE_CONNSTR} \
    --destination ${ASSETS_BLOB_STORAGE_CONTAINER} \
    --source src/arm-template/sample-data/public/item-service/images \
    --pattern "*.png"

# Get endpoint of assets storage
ASSETS_BLOB_STORAGE_URL=$(az storage account show \
    --name ${ASSETS_BLOB_STORAGE_NAME} \
    --query "primaryEndpoints.blob" \
    --output tsv)

# Set image paths into the source data
sed -i -e "s|https://sample.blob.core.windows.net/|${ASSETS_BLOB_STORAGE_URL}|g" src/arm-template/sample-data/public/item-service/itemMasterSampleData.json

# Insert documents to item-service Cosmos DB
ITEM_SERVICE_COSMOSDB_CONNSTR=$(az cosmosdb list-connection-strings \
    --resource-group ${RESOURCE_GROUP} \
    --name ${ITEM_SERVICE_COSMOSDB} \
    --query "connectionStrings[0].connectionString" \
    --output tsv)

<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\sample-data\\public\\item-service\\itemMasterSampleData.json \
    /t:DocumentDB \
    /t.ConnectionString:"${ITEM_SERVICE_COSMOSDB_CONNSTR};Database=${ITEM_SERVICE_COSMOSDB_DATABASE};" \
    /t.Collection:${ITEM_SERVICE_COSMOSDB_COLLECTION} \
    /t.PartitionKey:${ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY} \
    /t.CollectionThroughput:400
```

#### Box管理サービス・POSサービスのマスタの準備

Box管理サービス・POSサービスのマスタを Azure Cosmos DB に準備し、必要に応じてデータのを登録します。
まず、 Azure Cosmos DB (SQL API) にコレクションを作成します。

```bash
# Set variables following above, if you did not set them
RESOURCE_GROUP=<resource group name>
POS_DB_ACCOUNT_NAME=${PREFIX}-pos-service
BOX_DB_ACCOUNT_NAME=${PREFIX}-box-service

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
    /s.Files:.\\src\\arm-template\\sample-data\\public\\pos-service\\PosMasters.json \
    /t:DocumentDB \
    /t.ConnectionString:"${POS_SERVICE_COSMOSDB_CONNSTR};Database=${POS_DB_NAME};" \
    /t.Collection:PosMasters
<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Skus.json \
    /t:DocumentDB \
    /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" \
    /t.Collection:Skus
<your-dt-command-path>/dt.exe \
    /s:JsonFile \
    /s.Files:.\\src\\arm-template\\sample-data\\public\\box-service\\Terminals.json \
    /t:DocumentDB \
    /t.ConnectionString:"${BOX_SERVICE_COSMOSDB_CONNSTR};Database=${BOX_DB_NAME};" \
    /t.Collection:Terminals
```