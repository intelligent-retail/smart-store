# Azure Notification Hub でのプッシュ通知の環境構築

プッシュ通知の環境構築には Azure Notification Hub と Google Firebase の設定が必要になります。ここでは Azure Notification Hub と Google Firebase Cloud Messaging（FCM） の設定方法を説明します。

以下の手順をおこなうことでプッシュ通知の環境構築をおこなうことができます。

1. Google Firebase でプロジェクトを作成する
1. Android のパッケージ名を取得する
1. Google Firebase で Cloud Messaging を追加する
1. Azure Notification Hub を作成する
1. Azure Notification Hub に FCM の構成を設定する
1. (ローカルビルドをする場合) Azure Notification Hub の接続情報を Xamarin のプロジェクトに設定する
1. プッシュ通知をテスト送信する

## 1. Google Firebase でプロジェクトを作成する

- [Google Firebase](https://firebase.google.com/?hl=ja) にログインします
- 右上の「コンソールへ移動」をクリックします
- 「プロジェクトを追加」をクリックし、下記を参考にプロジェクトを作成します
  - 「プロジェクト名」に任意の名前を入力します
  - 「このプロジェクトで Google アナリティクスを有効にする」のトグルは、必要に応じてチェックをしてください。有効にする場合は Google アナリティクスアカウントの選択または新規作成を促されます。
![](images/notification-hubs-005.png)

## 2. Android のパッケージ名を取得する

### Visual Studio で Android のパッケージ名を確認する場合

- Visual Studio で、 `src/arm-template/client-app/SmartRetailApp/SmartRetailApp.sln` を開きます
- 「Solution Explorer」の Android プロジェクト（Androidスマートフォンのようなアイコン）の `SmartRetailApp.Android` をクリックします
- 開いた画面の左メニューから「Android Manifest」を開きます
- 「Package name:」の値をコピーしておきます

### コードから Android のパッケージ名を確認する場合

- `src\client-app\SmartRetailApp\SmartRetailApp\SmartRetailApp.Android\Properties\AndroidManifest.xml` を開きます
- `<manifest>` タグの `package` プロパティの値をコピーしておきます

## 3. Google Firebase で Cloud Messaging を追加する

- Google Firebase コンソール（の左上）の設定→「プロジェクトの設定」をクリックします
- 「General」（または、「全般」）タブの「Your apps」（または、「マイアプリ」）で、Androidのキャラクターのアイコンをクリックします
- 「Android パッケージ名」に、4. で取得したパッケージ名を貼り付けます
- 「アプリを登録」をクリックすると `google-services.json` をダウンロードできるのでこれを保存しておきます（Xamarin プロジェクトで使用します）
![](images/notification-hubs-006.png)

## 4. Azure Notification Hub に FCM の構成を設定する

- Google Firebase の設定画面の「クラウドメッセージング」の「サーバーキー」をコピーします
- Azure Portal の Notification Hub から 「Google (GCM/FCM)」をクリックします
- サーバーキーをペーストします
- 「Save」をクリックします
![](images/notification-hubs-007.png)
![](images/notification-hubs-008.png)


## 5. Azure Notification Hub の接続情報を Xamarin のプロジェクトに設定する

SmartRetailApp の [README](/src/client-app/README.md) をご参考ください。

## 6. (任意) プッシュ通知をテスト送信する

Azure Notification Hub を通したプッシュ通知のテストを行うには、 [クライアントアプリ（SmartRetailApp）用プッシュ通知テストツール](/src/test/SendPush.Sample/README.md) をご参考ください。

## 参考

参考: [Get Started with Xamarin](https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/xamarin)
