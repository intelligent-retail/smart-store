# ボックス管理クライアントアプリ

## アーキテクチャ概要

[Xamarin.Forms](https://docs.microsoft.com/ja-jp/xamarin/xamarin-forms/) を採用することでコードを共通化し Android と iOS のハイブリッドアプリを実現しています。カート情報の自動更新には [App Center](https://docs.microsoft.com/ja-jp/appcenter/push/) のプッシュ通知機能を採用しました。

![client-app overview](images/client-app-overview.png)

## 主な特長

- Xamarin.Forms によるハイブリッドアプリ
  - Xamarin.Forms でほとんどのコードを共通化し、Android と iOS のハイブリッドアプリを実現。
- プッシュ通知によるカート情報の自動更新
  - App Center のプッシュ通知機能を活用してカート情報の更新や画面遷移を自動化。
- QR コード情報をカメラ機能で取得
  - カメラ機能を使って QR コードの値を取得（ QR コード情報の読み取りには ZXing.Net.Mobile ライブラリを使用）

## 参考
- [Xamarin.Forms](https://docs.microsoft.com/ja-jp/xamarin/xamarin-forms/)

- [App Center Push](https://docs.microsoft.com/en-us/appcenter/push/)

- [ZXing.Net.Mobile](https://github.com/Redth/ZXing.Net.Mobile)
