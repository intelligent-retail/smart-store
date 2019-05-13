# 動作確認

## 動作確認に必要な環境

- [Swagger UI](https://swagger.io/tools/swagger-ui/)
- Boxシミュレータ（現時点では非公開のため、利用されたい方はお問い合わせください。）

### あると便利なツール

- [Postman](https://www.getpostman.com/)
- IoTHub Device Explorer
  - [How to use Device Explorer for IoT Hub devices](https://github.com/Azure/azure-iot-sdk-csharp/tree/master/tools/DeviceExplorer)

## API の動作確認について

このプロジェクトに含まれるAPIは、OpenAPI に基づいたフォーマットで定義しており、Swagger UI などのGUI ツールで閲覧することができます。

API 定義ファイルの利用法については、下記をご参考ください。

- [API 定義ファイルの利用方法](/docs/api/README.md)

## 商品マスタの動作確認

### 商品マスタ 定義ファイル

Swagger UI で下記定義ファイルを開き、動作確認を行います。

- 商品マスタ取得API 定義
  - 相対パス: [/docs/api/item-master-api.yaml](/docs/api/item-master-api.yaml)
  - 外部参照用URL: https://raw.githubusercontent.com/intelligent-retail/smart-store/master/docs/api/item-master-api.yaml

### 商品マスタ取得API の動作確認

#### 商品マスタ取得API の動作確認 手順

1. `functionName` に `<your prefix>-item-master-api` にように設定する
2. 「商品マスタの取得」と書かれたAPIを開く
3. パラメータを確認し、「Execute」ボタンをクリックして実行する

#### 商品マスタ取得API の動作確認 期待結果

_Server response_ の下記項目を確認する。

- _Code_ が `200` であること
- _Response body_ として商品一覧が返却されること
- _Response body_ の商品の `itemCode` がパラメータで指定したコードであること

## 在庫管理サービスの動作確認

### 在庫管理サービス 定義ファイル

- 在庫トランAPI 定義
  - 相対パス: [/docs/api/stock-command-api.yaml](/docs/api/item-master-api.yaml)
  - 外部参照用URL: https://raw.githubusercontent.com/intelligent-retail/smart-store/master/docs/api/stock-command-api.yaml
- 在庫照会API 定義
  - 相対パス: [/docs/api/stock-query-api.yaml](/docs/api/item-master-api.yaml)
  - 外部参照用URL: https://raw.githubusercontent.com/intelligent-retail/smart-store/master/docs/api/stock-query-api.yaml

### 在庫トランAPI の動作確認

#### 在庫トランAPI の動作確認 手順

1. `functionName` に `<your prefix>-stock-command-api` にように設定する
2. 「在庫トランザクションの更新」と書かれたAPIを開く
3. パラメータを確認し、「Execute」ボタンをクリックして実行する

#### 在庫トランAPI の動作確認 期待結果

_Server response_ の下記項目を確認する。

- _Code_ が `200` であること
- _Response body_ として `OK` が返却されること

### 在庫照会API の動作確認

#### 在庫照会API の動作確認 手順

1. `functionName` に `<your prefix>-stock-query-api` にように設定する
2. 「在庫照会API(商品別)」と書かれたAPIを開く
3. パラメータを確認し、「Execute」ボタンをクリックして実行する

#### 在庫照会API の動作確認 期待結果

_Server response_ の下記項目を確認する。

- _Code_ が `200` であること
- _Response body_ の `itemCode` がパラメータで指定したコードであること
- _Response body_ の `quantity` が在庫トランAPIを実行した回数分のマイナス値であること

## POS サービスの疎通確認

作成した POS サービスが正常に動作するか、疎通確認を行います。
全体のシーケンスは以下のドキュメントをご参照ください。

- [SmartStore シーケンス図](/docs/images/smartStore-sequenceDiagram.png)

### POS サービス 定義ファイル

以下の POS サービス API 定義ドキュメントをご参照頂き、functionName (※) の設定、Authorize を行ってください。

- POS サービス API 定義
  - 相対パス: [/docs/api/pos-service-api.yaml](/docs/api/pos-service-api.yaml)
  - 外部参照用URL: https://raw.githubusercontent.com/intelligent-retail/smart-store/master/docs/api/pos-service-api.yaml

※ `functionName` は `<your prefix>-pos-api` となります。  
　`apiVersion` については既定値の `v1` から変更しません。

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
