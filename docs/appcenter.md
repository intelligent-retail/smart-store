## App Center でのプッシュ通知の環境構築
プッシュ通知の環境構築には App Center と Firebase の設定が必要になります。ここでは App Center Push と Firebase Cloud Messaging の設定方法を説明します。

以下の手順をおこなうことでプッシュ通知の環境構築をおこなうことができます。

1. App Center でアプリケーションを追加する
2. App Center で Push 環境を設定する
3. Firebase で プロジェクトを作成する
4. Firebase で Cloud Messaging を追加する
5. Firebase と App Center Push を関連付ける

### 1. App Center でアプリケーションを追加する

- [App Center](https://appcenter.ms/) にログインします
- `All apps` から右上の `Add new` → `Add new app` をクリックします
- App name: でアプリケーション名の入力、OS: は Android、Platform: は Xamarin を選択します
- 右下の `Add new app` をクリックします

![](images/appcenter-001.png)

### 2. AppCenter で Push 環境を設定する

- 作成したアプリケーションを選択して、Push をクリックします
- Xamarin 用のコードをコピーします（Xamarin プロジェクトで使用します）

### 3. Firebase でプロジェクトを作成する

- [Firebase](https://firebase.google.com/?hl=ja) にログインします
- 右上の「コンソールへ移動」をクリックします
- 「プロジェクトを作成」をクリックし、下記を参考にプロジェクトを作成します
  - 「プロジェクト名」に任意の名前を入力します
  - 「地域/ロケーション」は任意で指定します（例: 「アナリティクスの地域」を `日本` 、 「Cloud Firestore のロケーション」を `asia-northeast1` )
  - 「Firebase 向け Google アナリティクスのデータ共有にデフォルトの設定を使用する」と「測定管理者間のデータ保護条項に同意します」については、必要に応じてチェックをしてください。

![](images/appcenter-005.png)

### 4. Android のパッケージ名を取得する

#### Visual Studio で Android のパッケージ名を確認する場合

- Visual Studio で、 `src/arm-template/client-app/SmartRetailApp/SmartRetailApp.sln` を開きます
- 「Solution Explorer」の Android プロジェクト（Androidスマートフォンのようなアイコン）の `SmartRetailApp.Android` をクリックします
- 開いた画面の左メニューから「Android Manifest」を開きます
- 「Package name:」の値をコピーしておきます

#### コードから Android のパッケージ名を確認する場合

- `src\client-app\SmartRetailApp\SmartRetailApp\SmartRetailApp.Android\Properties\AndroidManifest.xml` を開きます
- `<manifest>` タグの `package` プロパティの値をコピーしておきます

### 5. Firebase で Cloud Messaging を追加する

- Firebase コンソール（の左上）の設定→「プロジェクトの設定」をクリックします
- 「General」（または、「全般」）タブの「Your apps」（または、「マイアプリ」）で、Androidのキャラクターのアイコンをクリックします
- 「Android パッケージ名」に、4. で取得したパッケージ名を貼り付けます
- 「アプリを登録」をクリックすると `google-services.json` をダウンロードできるのでこれを保存しておきます（Xamarin プロジェクトで使用します）

![](images/appcenter-006.png)

### 6. Firebase と App Center Push を関連付ける

- Firebase の設定画面の「クラウドメッセージング」の「サーバーキー」をコピーします
- App Center の　Push Notificatins を開き、「Next」ボタンをクリックします
- 「SET UP FIREBASE」では、上記で作業したのでなにもせず「Next」ボタンをクリックします
- 「ADD KEY」では、 `Add Server Key` の欄にコピーしたサーバーキーをペーストし、「Done」ボタンをクリックします

![](images/appcenter-007.png)

参考: [Get Started with Xamarin](https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/xamarin)
