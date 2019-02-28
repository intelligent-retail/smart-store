# client-app

`client-app` には `Xamarin.Forms` で構成された、クライアントアプリを実行するための以下のサンプル実装が含まれています。クライアントアプリの概要は [こちら](../../docs/client-app.md) を参照してください。

## SmartRetailApp

クライアントアプリの共通ライブラリです。以下の `SmartRetailApp.Android` と `SmartRetailApp.iOS` で共通で使用される `XAML` によるUI や `API` によるデータ取得、プッシュ通知などほとんどの機能が実装されています。

## SmartRetailApp.Android

`Android` で実行するためのプロジェクトです。

## SmartRetailApp.iOS

`iOS` で実行するためのプロジェクトです。

## プロジェクトをビルドするために必要な作業
プロジェクトをビルドするために以下の作業が必要になります。
1. `Constant.cs` を作成して `SmartRetailApp` プロジェクトに追加してください。

```c#
public class Constant
{
  public const string CartsApiName = "カート内容を更新するAPIのURL";
  public const string ApiKey = "APIのキー";
  public const string AppCenterKeyAndroid = "AppCenter(Android)のキー";
  public const string AppCenterKeyiOS = "AppCenter(iOS)のキー";
}
```

2. `google-services.json` の追加（ `SmartRetailApp.Android` のみ）
   - [FireBase](https://console.firebase.google.com/) で作成したアプリから`google-services.json` をダウンロードして、`SmartRetailApp.Android` プロジェクトに追加してください。

## ライセンス

### AppCenter-SDK-DotNet
AppCenter-SDK-DotNet is released under the MIT License.
AppCenter-SDK-DotNet can be found here: [https://github.com/Microsoft/AppCenter-SDK-DotNet](https://github.com/Microsoft/AppCenter-SDK-DotNet)
A copy of the MIT License can be found here: [https://github.com/Microsoft/AppCenter-SDK-DotNet/blob/develop/license.txt](https://github.com/Microsoft/AppCenter-SDK-DotNet/blob/develop/license.txt)

### Newtonsoft.Json
Newtonsoft.Json is released under the MIT License.
Newtonsoft.Json can be found here: [https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
A copy of the MIT License can be found here: [https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)

### Xamarin.Forms
Xamarin.Forms is released under the MIT License.
Xamarin.Forms can be found here: [https://github.com/xamarin/Xamarin.Forms](https://github.com/xamarin/Xamarin.Forms)
A copy of the MIT License can be found here: [https://github.com/xamarin/Xamarin.Forms/blob/master/LICENSE](https://github.com/xamarin/Xamarin.Forms/blob/master/LICENSE)


### ZXing.Net.Mobile
ZXing.Net.Mobile is released under the Apache License 2.0.
ZXing.Net.Mobile can be found here: [https://github.com/xamarin/Xamarin.Forms](https://github.com/xamarin/Xamarin.Forms)
A copy of the Apache License 2.0 can be found here: [https://github.com/Redth/ZXing.Net.Mobile/blob/master/LICENSE.txt](https://github.com/Redth/ZXing.Net.Mobile/blob/master/LICENSE.txt)
