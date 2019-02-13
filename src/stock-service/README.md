# stock-service

`stock-service` には在庫管理サービスを実現する以下のサンプル実装が含まれています。在庫管理サービスの概要は [こちら](../../docs/stock-service.md) を参照してください。

## StockService.Core

在庫管理サービスのコアライブラリです。主にデータストアにアクセスするためのPOCOやDBコンテキストが実装されています。

## StockService.StockCommand

在庫トランザクションを更新するための API を Azure Functions(C#) で実装しています。CQRSパターンの **Command** の役割を担っています。
バックエンドのデータストアは Azure Cosmos DB を想定しています。

現在実装されているAPIは以下です。

- 在庫更新API: `/api/v1/stocks`

## StockService.StockProcessor

CQRSパターンの書き込みストアと読み取りストアをほぼリアルタイムに同期させるため、Azure Cosmos DB の Change Feed と Azure Functions の Cosmos DB Trigger を使って、クエリ側のデータストアである Azure SQL DB へのデータ送信を実装しています。

なお、この機能には外部からアクセスできるAPIは存在しません。

## StockService.StockQuery

在庫データを照会するための API を Azure Functions(C#) で実装しています。CQRSパターンの **Query** の役割を担っています。
バックエンドのデータストアは Azure SQL DB を想定しています。

現在実装されているAPIは以下です。

- 在庫照会API(商品別): `/api/v1/query/item`
- 在庫照会API(ストア別): `/api/v1/company/{companyCode}/store/{storeCode}/query`
- 在庫照会API(ターミナル別): `/api/v1/company/{companyCode}/store/{storeCode}/terminal/{terminalCode}/query`
