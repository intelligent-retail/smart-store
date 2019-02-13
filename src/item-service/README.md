# item-service

`item-service` には統合商品マスターを実現する以下のサンプル実装が含まれています。
統合商品マスターの概要は [こちら](../../docs/item-master.md) を参照してください。

## `ItemService.ItemMaster`

統合商品マスターにアクセスするための API を Azure Functions(C#) で実装しています。
バックエンドのデータストアは Azure Cosmos DB を想定しています。

現在実装されているAPIは以下です。

- 商品情報取得API: `/v1/company/{company-code}/store/{store-code}/items`