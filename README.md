# Smart Store sample application

このリポジトリは、 [Smart Store リファレンスアーキテクチャー](https://news.microsoft.com/ja-jp/2019/01/29/blog-smart-store/) に基づくサンプル実装です。

:warning: _This document supports in Japanese only for now, sorry._

## Key Features

このサンプル実装には以下の機能が含まれています。

- 統合商品マスタ: `/src/item-service/`
- 在庫管理: `/src/stock-service`

## Getting Started

Azure へリソースをデプロイします。

Azure CLI を利用しますので、下記を参考に環境をご準備ください。

- [Azure CLI](https://docs.microsoft.com/ja-jp/cli/azure)

Azure CLI が準備できましたら、下記を参考にリソースをデプロイしてください。

```bash
RESOURCE_GROUP=<resource group name>
LOCATION=japaneast

PREFIX=<prefix string within 2 characters>
SQL_SERVER_ADMIN_PASSWORD=<sql server admin password>

# リソースグループを作成する
az group create \
    --name ${RESOURCE_GROUP} \
    --location ${LOCATION}

# 作成したリソースグループの中に、リソースをデプロイする
az group deployment create \
  --resource-group ${RESOURCE_GROUP} \
  --template-file src/arm-template/template.json \
  --parameters @src/arm-template/parameters.json \
  --parameters \
        prefix=${PREFIX} \
        sqlServerAdminPassword=${SQL_SERVER_ADMIN_PASSWORD}
```
