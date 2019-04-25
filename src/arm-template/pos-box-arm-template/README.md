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

TEMPLATE_URL=https://raw.githubusercontent.com/intelligent-retail/smart-store/master/src/arm-template

ITEM_MASTER_URI=https://$(az functionapp show --resource-group ${RESOURCE_GROUP} --name ${PREFIX}-item-master-api --query "defaultHostName" --output tsv)/api/v1/company/{company-code}/store/{store-code}/items
STOCK_COMMAND_URI=https://$(az functionapp show --resource-group ${RESOURCE_GROUP} --name ${PREFIX}-stock-command-api --query "defaultHostName" --output tsv)/api/v1/stocks

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
    --template-uri ${TEMPLATE_URL}/template.json \
    --parameters ${TEMPLATE_URL}/parameters.json \
    --parameters \
        prefix=${PREFIX} \
        itemMasterUri=${ITEM_MASTER_URI} \
        stockUri=${STOCK_COMMAND_URI} \
        notificationApiKey=${NOTIFICATION_API_KEY} \
        notificationUri=${NOTIFICATION_URI} \
        posApiKey=${POS_API_KEY}

# item-service と stock-service の api key を pos-api に設定する
ITEM_MASTER_API_KEY=<item service api key>
STOCK_COMMAND_API_KEY=<stock service command api key>
az functionapp config appsettings set \
    --resource-group ${RESOURCE_GROUP} \
    --name ${PREFIX}-pos-api \
    --settings \
        ItemMasterApiKey=${ITEM_MASTER_API_KEY} \
        StockApiKey=${STOCK_COMMAND_API_KEY}
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

## IoT Hub と Box管理サービスの紐づけ

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

### Box管理サービス・POSサービスのマスタの準備

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

## POS サービスの疎通確認

作成した POS サービスが正常に動作するか、疎通確認を行います。  
全体のシーケンスは以下のドキュメントをご参照ください。  
[SmartStore シーケンス図](/docs/images/smartStore-sequenceDiagram.png)

疎通確認には Swagger を使用します。Swagger の基本的な使用方法は以下のドキュメントをご参照ください。  
[API 定義ファイルの利用方法](/docs/api/README.md)

以下の POS サービス API 定義ドキュメントをご参照頂き、functionName (※) の設定、Authorize を行ってください。  
[POS サービス API 定義](/docs/api/pos-service-api.yaml)

※functionName は \<your prefix\>-pos-api となります。  
　apiVersion については既定値の v1 から変更しません。

### 1.カート作成要求 API

Request body にパラメータを設定し Execute を押します。  
※パラメータは以下の既定値から変更しません。

``` JSON
{
  "companyCode": "00100",
  "storeCode": "12345",
  "terminalNo": 1,
  "userId": "1",
  "userName": "テストユーザ"
}
```

Server response として Code 201 (Created)、Response body に以下のような値が返ってくれば成功です。

``` JSON
{
  "cartId": "598f75b8-d124-4395-937b-120775a99e34"
}
```

※cartId はランダムに決定されるため、上記 cartId はサンプル値です。毎回異なる値が返ってきます。

これでカートが作成され、カートの ID が割り振られたことになりますので、  
以降の API ではこの cartId を使用してカートの操作を行います。

### 2.カート状態取得 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
Server response として Code 200 (OK)、Response body に以下のような値が返ってくれば成功です。

``` JSON
{
  "store": {
    "storeCode": "12345",
    "storeName": "Smart Retail 六本木店",
    "terminalNo": 1
  },
  "user": {
    "userId": "1",
    "userName": "テストユーザ"
  },
  "cart": {
    "cartId": "598f75b8-d124-4395-937b-120775a99e34",
    "totalAmount": 0,
    "subtotalAmount": 0,
    "totalQuantity": 0,
    "receiptNo": 1,
    "receiptText": "",
    "depositAmount": 0,
    "changeAmount": 0,
    "balance": 0,
    "transactionNo": 1,
    "cartStatus": "01",
    "lineItems": [],
    "payments": [],
    "taxes": []
  }
}
```

※cartStatus で扱う状態は以下のものです。  
　01：商品登録  
　02：小計  
　03：取引完了  
　04：取引中止

### 3.取引中止 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると  
　「receiptText」に「取引中止＜売上＞」といった文言を含むレシート情報  
　「cartStatus」に 04 (取引中止)  
が設定されたカート情報を取得できます。  
※カート情報は 7 日間保持された後、自動的に消えます。

取引を中止したカートのため、これ以降は「2.カート状態取得 API」以外の API 受け付けはできなくなります。  
(実行した場合は Code 400 (Bad Request) が返ってきます)

### 4.商品追加 API

※もう一度「1.カート作成要求 API」を実行し、新たなカートを作成したものとして進めます。

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、  
Request body に以下のパラメータを設定し Execute を押します。

``` JSON
{
  "items": [
    {
      "itemCode": "4901427401646",
      "quantity": 2
    }
  ]
}
```

Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると  
　「lineItems」に数量 ２ の筆ペン  
　「totalAmount」に 1200  
が設定される等、カートに商品が追加されたことを確認できます。

### 5.商品削除 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力、  
「itemCode」に 4901427401646 を入力、「quantity」に 1 を入力し Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると「lineItems」の筆ペンが数量 1 になっていることが確認できます。

### 6.小計 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、  
Request body に以下のパラメータを設定し Execute を押します。

``` JSON
{
  "items": [
    {
      "itemCode": "4901427401646",
      "quantity": 1
    }
  ]
}
```

Server response として Code 200 (OK) が返ってくれば成功です。  
この状態で「2.カート状態取得 API」を実行すると「cartStatus」が 02 (小計) になっていることが確認できます。

### 7.支払い追加 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
パラメータは以下の既定値から変更しません。

``` JSON
{
  "payments": [
    {
      "paymentCode": "01",
      "amount": 0
    }
  ]
}
```

Server response として Code 200 (OK) が返ってくれば成功です。  
この状態で「2.カート状態取得 API」を実行すると以下の支払い情報が設定されていることが確認できます。

``` JSON
"payments": [
  {
    "paymentNo": 1,
    "paymentCode": "01",
    "paymentName": "クレジット",
    "paymentAmount": 600
  }
],
```

### 8.取引確定 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると
　「receiptText」に「領 収 書」といった文言を含むレシート情報  
　「cartStatus」に 03 (取引完了)  
が設定されたカート情報を取得できます。  

取引を完了したカートのため、これ以降は「2.カート状態取得 API」以外の API 受け付けはできなくなります。  
(実行した場合は Code 400 (Bad Request) が返ってきます)
