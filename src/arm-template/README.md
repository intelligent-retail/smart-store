# Deploy resources with ARM template

## デプロイ用環境について

- Visual Studio
- Visual Studio Code
- Azure CLI
  - `az extension add --name azure-cli-iot-ext`
- git
- `sqlcmd` ユーティリティ
- Data migration tool ( `dt` コマンド)

### `sqlcmd` ユーティリティ について

`sqlcmd` ユーティリティは、プロビジョニング用スクリプト ( _provision.ps1_ または _provision.sh_ ) の中で使用します。

インストールする際は、下記をご参考ください。

- Windows: [sqlcmd ユーティリティ](https://docs.microsoft.com/ja-jp/sql/tools/sqlcmd-utility?view=sql-server-2017)
- Linux: [sqlcmd および bcp、SQL Server コマンド ライン ツールを Linux にインストールする](https://docs.microsoft.com/ja-jp/sql/linux/sql-server-linux-setup-tools?view=sql-server-2017)

### Data migration tool ( `dt` コマンド) について

Data migration tool ( `dt` コマンド) は、Cosmos DB に対してデータをアップロードする際に使用します。プロビジョニング用スクリプト ( _provision.ps1_ ) の中で使用しています。

インストールする際は、下記を参考に実行ファイルを展開し、`dt` に対してパスが通るようにしておきましょう。

- [データ移行ツール ( `dt` コマンド) を使用して Azure Cosmos DB にデータを移行する](https://docs.microsoft.com/ja-jp/azure/cosmos-db/import-data)

なお、 この Data migration tool は Windows でしか動作しないので、Linux で作業する場合は別の方法で Cosmos DB にデータをアップロードしてください。（後述）下記は参考です。

- [(ポータルを用いた) サンプル データの追加](https://docs.microsoft.com/ja-jp/azure/cosmos-db/create-sql-api-dotnet#add-sample-data)
- [Azure Cosmos DB Bulk Executor ライブラリの概要](https://docs.microsoft.com/ja-jp/azure/cosmos-db/bulk-executor-overview)

## 全体の流れ

- ARMテンプレートでデプロイする
- プロビジョニングする
- 各 Functions に API key を設定する
- App Center たてる
- Azure Functions の Application Settings に設定を追加する
  - 各 API key
  - App Center の URL とキー
- Visual Studio で pos-service と box-service の Functions をデプロイする

## リソースのデプロイ

Azure へリソースをデプロイします。

Azure CLI を利用しますので、下記を参考に環境をご準備ください。

- [Azure CLI](https://docs.microsoft.com/ja-jp/cli/azure)

Azure CLI が準備できましたら、下記を参考にリソースをデプロイしてください。

### PowerShell によるデプロイ

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
```

### bash によるデプロイ

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
```

## プロビジョニング

※ 変数は前項から引き継いでるものとします。

### PowerShell によるプロビジョニング

```ps1
# まだリポジトリをクローンしていない場合は、クローンする
git clone https://github.com/intelligent-retail/smart-store.git

# リポジトリのディレクトリに移動する
cd smart-store

# プログラムの実行権限を確認する
Get-ExecutionPolicy -List

# 上記で CurrentUser に RemoteSigned が当たってない場合は、下記を実行する
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# プロビジョニングを実行する
.\src\arm-template\provision.ps1
```

### bash によるプロビジョニング

_準備中_

```bash
# まだリポジトリをクローンしていない場合は、クローンする
git clone https://github.com/intelligent-retail/smart-store.git

# リポジトリのディレクトリに移動する
cd smart-store

# プロビジョニングを実行する
./src/arm-template/provision.sh
```


## Azure Functions の Application Settings の設定追加

※ 変数は前項から引き継いでるものとします。

### PowerShell による Azure Functions の Application Settings の更新

```ps1
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

### bash による Azure Functions の Application Settings の更新

```bash
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

### 各種マスタの準備

### 商品データに画像を含める場合の事前準備

必要に応じて、画像のアップロードおよびデータの登録を行います。

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
   - `ITEM_SERVICE_COSMOSDB`
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

つぎに、データの登録を行います。  
データ移行は様々な方法が提供されています。ここでは、以下の作業を Azure CLI およびデータ移行ツール ( `dt` コマンド) を用いて、コマンドラインで実施する方法をご紹介します。インポートファイルを用意しておりますが、適宜読み替えてご参考ください。

- [データ移行ツール ( `dt` コマンド) を使用して Azure Cosmos DB にデータを移行する](https://docs.microsoft.com/ja-jp/azure/cosmos-db/import-data)

```bash
# Set variables following above, if you did not set them
POS_DB_ACCOUNT_NAME=${PREFIX}-pos-service
BOX_DB_ACCOUNT_NAME=${PREFIX}-box-service
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

## 動作確認

### 動作確認に必要な環境

- Swagger UI
  - https://swagger.io/tools/swagger-ui/
- Boxシミュレータ（現時点では非公開のため、利用されたい方はお問い合わせください。）

#### あると便利なツール

- [Postman](https://www.getpostman.com/)
- IoTHub Device Explorer
  - [How to use Device Explorer for IoT Hub devices](https://github.com/Azure/azure-iot-sdk-csharp/tree/master/tools/DeviceExplorer)

### POS サービスの疎通確認

作成した POS サービスが正常に動作するか、疎通確認を行います。  
全体のシーケンスは以下のドキュメントをご参照ください。  
[SmartStore シーケンス図](/docs/images/smartStore-sequenceDiagram.png)

疎通確認には Swagger を使用します。Swagger の基本的な使用方法は以下のドキュメントをご参照ください。  
[API 定義ファイルの利用方法](/docs/api/README.md)

以下の POS サービス API 定義ドキュメントをご参照頂き、functionName (※) の設定、Authorize を行ってください。  
[POS サービス API 定義](/docs/api/pos-service-api.yaml)

※functionName は \<your prefix\>-pos-api となります。  
　apiVersion については既定値の v1 から変更しません。

#### 1.カート作成要求 API

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

#### 2.カート状態取得 API

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

#### 3.取引中止 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると  
　「receiptText」に「取引中止＜売上＞」といった文言を含むレシート情報  
　「cartStatus」に 04 (取引中止)  
が設定されたカート情報を取得できます。  
※カート情報は 7 日間保持された後、自動的に消えます。

取引を中止したカートのため、これ以降は「2.カート状態取得 API」以外の API 受け付けはできなくなります。  
(実行した場合は Code 400 (Bad Request) が返ってきます)

#### 4.商品追加 API

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

#### 5.商品削除 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力、  
「itemCode」に 4901427401646 を入力、「quantity」に 1 を入力し Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると「lineItems」の筆ペンが数量 1 になっていることが確認できます。

#### 6.小計 API

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

#### 7.支払い追加 API

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

#### 8.取引確定 API

Parameters「cartId」に「1.カート作成要求 API」で得た cartId を入力し、Execute を押します。  
Server response として Code 200 (OK) が返ってくれば成功です。

この状態で「2.カート状態取得 API」を実行すると
　「receiptText」に「領 収 書」といった文言を含むレシート情報  
　「cartStatus」に 03 (取引完了)  
が設定されたカート情報を取得できます。  

取引を完了したカートのため、これ以降は「2.カート状態取得 API」以外の API 受け付けはできなくなります。  
(実行した場合は Code 400 (Bad Request) が返ってきます)
