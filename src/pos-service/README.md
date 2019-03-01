# pos-service

`pos-service` にはPOSサービスを実現する以下のサンプル実装が含まれています。
POSサービスの概要は [こちら](../../docs/pos-service.md) を参照してください。

## `PosService`

POSサービスを実現する為の API を Azure Functions(C#) で実装しています。
バックエンドのデータストアは Azure Cosmos DB を想定しています。

現在実装されているAPIは以下です。

- カート作成要求API: `/v1/carts`
- カート状態取得API: `/v1/carts/{cartId}`
- 商品追加API: `/v1/carts/{cartId}/items`
- 商品削除API: `/v1/carts/{cartId}/items/{itemCode}`
- 小計API: `/v1/carts/{cartId}/subtotal`
- 支払い追加API: `/v1/carts/{cartId}/payments`
- 取引確定API: `/v1/carts/{cartId}/bill`
- 取引中止API: `/api/v1/carts/{cartId}`
