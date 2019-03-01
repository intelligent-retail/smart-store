# box-service

`box-service` にはBox管理サービスを実現する以下のサンプル実装が含まれています。
Box管理サービスの概要は [こちら](../../docs/box-service.md) を参照してください。

## `BoxManagementService`

Box管理サービスを実現する為の API を Azure Functions(C#) で実装しています。
バックエンドのデータストアは Azure Cosmos DB を想定しています。

現在実装されているAPIは以下です。

- カート作成要求API: `/v1/carts`
- カート状態取得(精算)API: `/v1/carts/{cartId}/bill`
- カート状態取得(明細)API: `/v1/carts/{cartId}/Items`
- Smart Box 状態初期化API: `/v1/devices/{deviceId}/status/reset`
