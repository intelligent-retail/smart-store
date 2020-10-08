# クライアントアプリ（SmartRetailApp）用プッシュ通知テストツール

本ツール SendPush.Sample を使用して、クライアントアプリ（SmartRetailApp）にプッシュ通知を送信しテストすることができます。（※ プッシュ通知を行うには実機をご用意ください。シミュレータでは動作しません。）

## 前提条件

本ツールは、本リポジトリの環境のもと動作します。

Azure のリソースのデプロイおよびプロビジョニングについては、 [セルフペースドハンズオン資料](/docs/self-paced-handson.md) をご参照ください。

クライアントアプリ（SmartRetailApp）については、 `client-app` の [README](/src/client-app/README.md) をご参照ください。

## 利用方法

まず、デバイス ID を取得します。

ビルド済みのクライアントアプリを実機にインストール・起動し、下記の方法でデバイス ID を取得します。

  - 最初の画面で「買い物を開始します」をタップした後、カメラの画面をキャンセル（戻る）し、デバッグ用にカート画面を表示する
  - 画面上部のテキストボックスの値をコピーする
  ![](images/notification-hubs-009.png)

つぎに、本プロジェクトの `src/test/SendPush.Sample/local.settings.sample.json` を `local.settings.json` にリネームし、下記の値を更新します。

- `NotificaitonHubConnectionStrings`: Azure Notification Hub の Access Policies の「box-service-full-access」の値（※）
- `NotificationHubName`: Notification Hub のハブ名の値

※ `box-service-full-access` は `DefaultFullSharedAccessSignature` と同等の権限（ Listen, Manage, Send ）を持つアクセスポリシーです。

```json
{
    "NotificaitonHubConnectionStrings": "Endpoint=sb://***.servicebus.windows.net/;SharedAccessKeyName=box-service-full-access;SharedAccessKey=***",
    "NotificationHubName": "<PREFIX>-box-service"
}
```

ターミナルで `src\test\SamplePush.Sample\SamplePush.Sample.csproj` のあるディレクトリへ移動し、下記コマンドを実行します。

```
cd src\test\SamplePush.Sample
dotnet build
dotnet run sendpush --deviceIdList {コピーしたデバイスID}
```

実行するアクション（カートの更新または清算）を聞かれるので選択します。

```
? Select Action
> update_cart
  receipt
```

正常に接続できていれば、デバイスで画面が遷移することを確認できます。
