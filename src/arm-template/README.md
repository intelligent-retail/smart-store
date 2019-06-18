# Deploy resources with ARM template and Azure CLI

## デプロイ用環境について

### インストールするソフトウェア

- Visual Studio
- Visual Studio Code
- Azure CLI
  - IoT 拡張機能のインストール
- `sqlcmd` ユーティリティ
- Data migration tool ( `dt` コマンド)
- git

### チェック項目

- Azure ポータルに自身のAzureアカウントでログインできていることを確認する
- `az account show` を実行し、Azure CLIで自身のAzureアカウントでログインできていることを確認する

### Visual Studio について

この手順では主に Azure Functions のデプロイに使用します。

インストールする際は、下記をご参考ください。

- [Downloads | IDE, Code, & Team Foundation Server | Visual Studio](https://visualstudio.microsoft.com/downloads/)

### Visual Studio Code について

この手順では、ドキュメントやソースコードの閲覧、編集に使用します。

インストールする際は、下記をご参考ください。

- [Visual Studio Code - Code Editing. Redefined](https://code.visualstudio.com/#alt-downloads)

### Azure CLI について

クロスプラットフォームで利用できる Azure CLI です。デプロイやプロビジョニングで使用します。

インストールする際は、下記をご参考ください。

- [Azure CLI のインストール | Microsoft Doc](https://docs.microsoft.com/ja-jp/cli/azure/install-azure-cli)

また、 IoT 拡張機能が必要になるので、下記を参考にインストールしてください。

```ps1
# IoT エクステンションをインストールする
az extension add --name azure-cli-iot-ext
```

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

## デプロイ作業の流れ

- ARMテンプレートでデプロイする
- スクリプトを用いてプロビジョニングする
- App Center を準備する
- 各 Functions に API key を設定する
- Azure Functions の Application Settings に設定を追加する
  - 各 API key
  - App Center の URL とキー
- Visual Studio で pos-service と box-service の Functions をデプロイする

## ARMテンプレートでデプロイする

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

## スクリプトを用いてプロビジョニングする

※ 変数は前項から引き継いでるものとします。

スクリプトを用いてプロビジョニングを行います。

スクリプトでは下記の処理を行っています。

- SQLデータベースのテーブル作成
- IoT Hub の IoT デバイスの登録
- IoT Hub とBOX管理サービスの紐づけ
- 各 Cosmos DB のデータベース、コレクション作成

### 実行前の確認

- `az extension show --name azure-cli-iot-ext` を実行し、Azure CLIにIoT拡張機能がインストールされていることを確認する
- `sqlcmd -?` を実行し、sqlcmdユーティリティがインストールされていることを確認する
- `dt` を実行し、dtコマンドがインストールされていることを確認する
  - インストールされていない場合は、 PowerShell のスクリプトは利用できません

### PowerShell によるプロビジョニング

```ps1
# まだリポジトリをクローンしていない場合は、クローンする
git clone https://github.com/intelligent-retail/smart-store.git

# リポジトリのディレクトリに移動する
cd smart-store

# 必要に応じて、pull しておく
git checkout master
git pull

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

# 必要に応じて、pull しておく
git checkout master
git pull

# プロビジョニングを実行する
./src/arm-template/provision.sh
```

## App Center を準備する

プッシュ通知の環境を準備します。下記をご参照ください。

- [App Center でのプッシュ通知の環境構築](/docs/appcenter.md)

## 各 Functions に API key を設定する

Azure Functions に API key を設定します。

Azure Functions の API key は、関数全体、または関数個別に設定することができます。ここでは、作業簡略化のため、同じ値のキーを関数全体に設定します。

1. Azureポータルで、デプロイした Auzre Functions のひとつを開き、「Function App の設定」を開きます。
2. 「Function App の設定」画面で、「ホスト キー（すべての関数）」の「新しいホスト キーの追加」ボタンをクリックします。
3. 「名前」の欄に `app` と入力し、「保存」ボタンをクリックして保存します。（値は空欄のままとし、自動生成させる）
4. 保存されたら、「アクション」欄の「コピー」をクリックし、生成されたキーをコピーします。

次に、コピーしたキーをほかの Azure Functions に設定します。

1. 他の Azure Functions を開き、「Function App の設定」画面に移動します。
2. 「ホスト キー（すべての関数）」の「新しいホスト キーの追加」ボタンをクリックします。
3. 「名前」に `app` 、「値」にコピーしたキーをはりつけて、「保存」ボタンをクリックし保存します。
4. その他の Azure Functions も同様に設定します。

## Azure Functions の Application Settings に設定を追加する

※ 変数は前項から引き継いでるものとします。

前項で設定した API key とプッシュ通知の情報を Azure Functions の Application Settings に追加します。

後述の手順のうち、下記の変数には、Azure Functions で設定した API key を指定してください。

- `ITEM_MASTER_API_KEY`
- `STOCK_COMMAND_API_KEY`
- `POS_API_KEY`

また、下記の変数には、それぞれプッシュ通知のキーとURLを指定してください。

- `NOTIFICATION_API_KEY`
- `NOTIFICATION_URI`

`NOTIFICATION_API_KEY` は、下記の手順で取得した値を貼り付けてください。

- App Center 右上のアイコンをクリックし、「Account settings」を開く
- 「Settings」の「API Tokens」を開く
- 右上の「New API token」ボタンをクリックする
- 下記を参考にトークンを発行する
  - 「Description」に任意の説明文を入力する
  - 「Access」で `Full Access` を選択する
  - 「Add new API token」ボタンをクリックし、発行する
- 「Here’s your API token.」で表示されたトークンをコピーしておく（一度しか表示されないのでご留意ください）

`NOTIFICATION_URI` は、下記の手順で取得した値を貼り付けてください。

- App Center で作成したアプリケーションを開く
- URLが下記のような構成になっているので、 `{owner_name}` と `{app_name}` の部分を取得する
  - `https://appcenter.ms/users/{owner_name}/apps/{app_name}`
- `NOTIFICATION_URI` 下記の URL の `{owner_name}` と `{app_name}` を置き換えて、`NOTIFICATION_URI` に設定する
  - `https://api.appcenter.ms/v0.1/apps/{owner_name}/{app_name}/push/notifications`

詳細は下記をご参考下さい。

- [Push | App Center API](https://openapi.appcenter.ms/#/push/Push_Send)
- [How to find the app name and owner name from your app URL | App Center Help Center](https://intercom.help/appcenter/general-questions/how-to-find-the-app-name-and-owner-name-from-your-app-url)


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
$NOTIFICATION_URI="https://api.appcenter.ms/v0.1/apps/{owner_name}/{app_name}/push/notifications"
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
NOTIFICATION_URI=https://api.appcenter.ms/v0.1/apps/{owner_name}/{app_name}/push/notifications
az functionapp config appsettings set \
  --resource-group ${RESOURCE_GROUP} \
  --name ${PREFIX}-box-api \
  --settings \
    NotificationApiKey=${NOTIFICATION_API_KEY} \
    NotificationUri=${NOTIFICATION_URI} \
    PosApiKey=${POS_API_KEY}
```

## Visual Studio で pos-service と box-service の Functions をデプロイする

ここでは、POS管理サービスとBOX管理サービスの Functions について、Visual Studio を使ってコードをデプロイします。

### POS管理サービスのコードをデプロイする

1. Visual Studio を起動する
2. `src/pos-service/PosService.sln` を開く
3. 「Solution Explorer」（または、「ソリューション エクスプローラー」）の `PosService` ソリューションの `PosService` プロジェクトを右クリックする
1. 「Publish」（または、「発行」）をクリックする
1. 「Pick a publish target」（または、「発行先を選択」）ダイアログで、「Azure Function App」（または、「Azure関数アプリ」）タブを開く
1. 「Select Existing」（または、「既存のものを選択」）を選択し、「Run from package file (recommended)」（または、「パッケージファイルから実行する（推奨）」）にチェックを付ける
1. 右下のプルダウンから「Create profile」（または、「プロファイルの作成」）を選択する
1. 「Subscription」「View」「Search」（または、「サブスクリプション」「表示」「検索」）を操作して、デプロイ先の Azure Functions として「<PREFIX>-pos-api」を選択し、「OK」ボタンをクリックする
1. 「Publish」（または、「発行」）画面で、作成したプロファイルが表示されていることを確認し、「Publish」（または、「発行」）ボタンをクリックする

### BOX管理サービスのコードをデプロイする

1. `src/box-service/BoxManagermentService.sln` を開く
2. 「Solution Explorer」の `BoxManagementService` ソリューションの `BoxManagementService` プロジェクトを右クリックする
3. 前述の手順と同様に操作し、「<PREFIX>-box-api」に Publish する

## 動作確認

動作確認は下記ドキュメントをご参照ください。

- [動作確認](/docs/operation-check.md)

## 備考

### スクリプトを使わない場合の各種マスタの準備

ここでは、手動でマスタを準備する方法をご紹介します。前項の [スクリプトを用いてプロビジョニングする](#スクリプトを用いてプロビジョニングする) でスクリプトで実施できる方は読み飛ばしてください。

#### 統合商品マスタの準備

Cosmos DB の操作は様々な方法が提供されていますので、適宜ご利用ください。

- Azure Cosmos DB へのインポート
  - [(ポータルを用いた) サンプル データの追加](https://docs.microsoft.com/ja-jp/azure/cosmos-db/create-sql-api-dotnet#add-sample-data)
  - [データ移行ツール ( `dt` コマンド) を使用して Azure Cosmos DB にデータを移行する](https://docs.microsoft.com/ja-jp/azure/cosmos-db/import-data)
  - [Azure Cosmos DB Bulk Executor ライブラリの概要](https://docs.microsoft.com/ja-jp/azure/cosmos-db/bulk-executor-overview)

ここでは、以下の作業を Azure CLI およびデータ移行ツール ( `dt` コマンド) を用いて、コマンドラインで実施する方法をご紹介します。

```bash
# Insert documents to item-service Cosmos DB
ITEM_SERVICE_COSMOSDB_DATABASE="00100"
ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT="400"
ITEM_SERVICE_COSMOSDB_COLLECTION="Items"
ITEM_SERVICE_COSMOSDB_COLLECTION_PARTITIONKEY="/storeCode"

ITEM_SERVICE_COSMOSDB=az cosmosdb list \
    --resource-group ${RESOURCE_GROUP} \
    --query "[?contains(@.name, 'item')==``true``].name" \
    --output tsv
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
    /t.CollectionThroughput:${ITEM_SERVICE_COSMOSDB_DATABASE_THROUGHPUT}
```

##### 商品データに画像を含める場合の事前準備

必要に応じて、画像のアップロードを行います。サンプルの画像とインポートファイルを対象に説明しますが、適宜読み替えてご参考ください。

1. `sample-data/public/item-service/images` ディレクトリ配下に格納されている png 画像を Azure Blog Storage にアップロードする
1. アップロードした URL をインポート用の JSON データに反映する

画像のアップロード操作は様々な方法が提供されていますので、適宜ご利用ください。

- Azure Blob Storage へのアップロード
  - [Azure portal を使用して BLOB をアップロード、ダウンロード、および一覧表示する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-portal)
  - [Azure Storage Explorer を使用してオブジェクト ストレージ内に BLOB を作成する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-storage-explorer)
  - [Azure CLI を使用して BLOB をアップロード、ダウンロード、および一覧表示する](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-cli)

ここでは Azure CLI を用いた方法を紹介します。

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
```

これで商品マスタのインポート用データに画像データを反映できたのので、 [統合商品マスタの準備](#統合商品マスタの準備) に戻り手順を実施してください。

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
