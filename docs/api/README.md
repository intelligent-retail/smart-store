# API定義ファイルの利用方法

## 概要

本プロジェクトにおけるAPI定義は、[OpenAPI Initiative](https://www.openapis.org/) によって管理されている OpenAPI (旧 Swagger) に従い記述しています。OpenAPI は、API定義の記述を統一するための規格です。

- [API Resources | Swagger](https://swagger.io/resources/open-api/)

また、 [Swagger](https://swagger.io/) は OpenAPI に関連するツールを提供しており、そのひとつの [Swagger UI](https://swagger.io/tools/swagger-ui/) はAPI定義を GUI で閲覧でき、API を実行することもできます。

## Swagger UI を利用する

ここでは、 Swagger UI の利用方法についてご紹介します。

- [Swagger UI の Live demo を利用する](#Swagger%20UI%20の%20Live%20demo%20を利用して%20Swagger%20UI%20を利用する)
- [Visual Studio Code の拡張機能を利用する](#Visual%20Studio%20Code%20の拡張機能を利用して%20Swagger%20UI%20を利用する)

### Swagger UI の Live demo を利用して Swagger UI を利用する

Swagger UI は自前でホストして利用するGUIツールですが、サイトで公開されている Live Demo は、定義ファイルのURLを指定することで定義を閲覧できるようになっています。この Live Demo を利用して定義を閲覧する方法をご紹介します。

なお、本プロジェクトの master ブランチにある定義ファイルは、下記URLで取得できます。

- `item-master-api.yaml` の例
  - https://raw.githubusercontent.com/intelligent-retail/smart-store/master/docs/api/item-master-api.yaml

1. [Swagger UI のページ](https://swagger.io/tools/swagger-ui/)を開き、「Live Demo」ボタンをクリックする
2. _pet store_ のサンプルが表示されます
3. 画面上部のURL入力欄に、定義ファイルのURLを入力し、「Explore」ボタンをクリックする

APIの実行は、[Swagger UI で API を実行する](#Swagger%20UI%20で%20API%20を実行する) をご参照ください。

### Visual Studio Code の拡張機能を利用して Swagger UI を利用する

Visual Studio Code では、 Swagger UI を利用できる拡張機能が公開されており、インストールするだけで簡単に利用することができます。

いくつかありますが、ここでは [openapi-designer](https://marketplace.visualstudio.com/items?itemName=philosowaffle.openapi-designer) を利用する手順をご紹介します。

#### 拡張機能 Swagger Viewer のインストール

1. [openapi-designer](https://marketplace.visualstudio.com/items?itemName=philosowaffle.openapi-designer) を開き、「Install」ボタンをクリックします。
1. _Visual Studio Code_ を開いてよいかメッセージが表示される場合は、許可します。
1. _Visual Studio Code_ が開き、 _openapi-designer_ のページが開くので、「Install」ボタンをクリックします。

#### 拡張機能 OpenAPI Designer でAPI定義を GUI で表示する（Swagger UI）

1. API定義ファイル（例: `docs/api/stock-command-api.yaml` ）を開きます。
1. `Ctrl + Shift + p` キーを押下し、 _Command Palette_ を開きます。
1. `OpenAPI Designer: Preview` と入力し、表示された選択候補を選択します。すると、右に新しいペインが作られ、 Swagger UI が表示されます。

![Screenshot: OpenAPI Designer of Visual Studio Code Extension](../images/guide-apidocs-vscode-ext-openapi-designer.png)

APIの実行は、[Swagger UI で API を実行する](#Swagger%20UI%20で%20API%20を実行する) をご参照ください。

## Swagger UI で API を実行する

Swagger UI で本プロジェクトのAPIを実行するには、下記のようにキーを設定する必要があります。

1. 利用したいAPI定義を開きます。
1. APIの認証設定が必要なので、「Authorize」ボタンをクリックします。
 ![Screenshot: How to use Swagger UI (001)](../images/guide-apidocs-try-api-001.png)
1. `x-functions-key` の `Value` にキー(※)を入力し、「Authorize」ボタンをクリックします。
 ![Screenshot: How to use Swagger UI (002)](../images/guide-apidocs-try-api-002.png)
   - ※ `x-functions-key` には、 Azure Function の API Key (_Function Key または Host Key_) の値を指定します。ポータルなどから取得するか、管理者にお問い合わせください。
1. 「Close」ボタンをクリックして閉じます。
 ![Screenshot: How to use  Swagger UI (003)](../images/guide-apidocs-try-api-003.png)
1. 試したいAPIをクリックして詳細を開き、「Try it out」ボタンをクリックします。
 ![Screenshot: How to use Swagger UI (004)](../images/guide-apidocs-try-api-004.png)
1. 必要に応じてパラメータを設定し、「Execute」ボタンをクリックすると、APIが実行されます。
 ![Screenshot: How to use Swagger UI (005)](../images/guide-apidocs-try-api-005.png)
