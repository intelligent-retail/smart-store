using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SmartRetailApp.Models;
using SmartRetailApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartRetailApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {

            InitializeComponent();

            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;

            edtBoxName.Text = "SmartBox1";
            // btnStartShopping.IsVisible = false;

            btnLoginLogout.Clicked += async (sender, e) =>
            {
                if (btnLoginLogout.Text == "ログアウト")
                {
                    await SignOut();
                }
                else
                {
                    var app = Application.Current as App;
                    var authResult = await app.SignInAsync();

                    if (authResult == null)
                    {
                        await DisplayAlert("ログインできませんでした", app.AuthErrorMessage, "OK");
                    }
                    else
                    {
                        JObject user = ParseIdToken(authResult.IdToken);

                        var msg = new StringBuilder();
                        msg.AppendLine($"Name: {user["name"]?.ToString()}");
                        msg.AppendLine($"User Identifier: {user["oid"]?.ToString()}");
                        msg.AppendLine($"Street Address: {user["streetAddress"]?.ToString()}");
                        msg.AppendLine($"City: {user["city"]?.ToString()}");
                        msg.AppendLine($"State: {user["state"]?.ToString()}");
                        msg.AppendLine($"Country: {user["country"]?.ToString()}");
                        msg.AppendLine($"Job Title: {user["jobTitle"]?.ToString()}");

                        if (user["emails"] is JArray emails)
                        {
                            msg.AppendLine($"Emails: {emails[0].ToString()}");
                        }
                        msg.AppendLine($"Identity Provider: {user["iss"]?.ToString()}");

                        await DisplayAlert("ログインしました", msg.ToString(), "OK");
                        btnLoginLogout.Text = "ログアウト";
                        // btnStartShopping.IsVisible = true;

                    }
                }
            };
        }

        protected override async void OnAppearing()
        {
            try
            {
                // Look for existing account
                IEnumerable<IAccount> accounts = await App.AuthenticationClient.GetAccountsAsync();

                AuthenticationResult result = await App.AuthenticationClient
                    .AcquireTokenSilent(Constant.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

            }
            catch
            {
                // Do nothing - the user isn't logged in
            }
            base.OnAppearing();
        }

        JObject ParseIdToken(string idToken)
        {
            // Parse the idToken to get user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }

        private string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        async Task SignOut()
        {
            var app = Application.Current as App;
            app.SignOut();

            btnLoginLogout.Text = "ログイン";
            // btnStartShopping.IsVisible = false;

            await DisplayAlert("ログアウトしました", "", "OK");
        }

        private async void LoginClicked(object sender, EventArgs e)
        {
            await ViewScanPageAsync();
        }

        /// <summary>
        /// インジケータ表示の切り替え
        /// </summary>
        /// <param name="isVisible"></param>
        void SetIndicator(bool isVisible)
        {
            if (isVisible)
            {
                loadingIndicator.VerticalOptions = LayoutOptions.CenterAndExpand;
                loadingIndicator.HeightRequest = 50;
                loadingIndicator.IsRunning = true;
                loadingIndicator.IsVisible = true;

                // 全体を隠す
                statckLayout.IsVisible = false;
            }
            else
            {
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;

                statckLayout.IsVisible = true;
            }
        }

        /// <summary>
        /// スキャンページに遷移する
        /// </summary>
        /// <returns></returns>
        private async Task ViewScanPageAsync()
        {
            try
            {
                // Box名を保存
                (Application.Current as App).BoxId = edtBoxName.Text;

                // インジケータを表示
                SetIndicator(true);

                // カメラでQRコードを撮影する
                var scanner = DependencyService.Get<IQrScanningService>();
                var scanResult = await scanner.ScanAsync();
                if (scanResult != null)
                {
                    (Application.Current as App).BoxId = scanResult.Text;
                }

                // デバイスIDを取得
                var deviceId = await AppCenter.GetInstallIdAsync();

                // 取引開始
                var api = new CartApiService();
                var cartResult = await api.CartStartAsync(new CartStart
                {
                    BoxId = (Application.Current as App).BoxId,
                    DeviceId = deviceId.ToString()
                });

                // 取引開始で商品カートへ遷移
                //if (cartResult != null && /*cartResult.IsSuccess*/ !string.IsNullOrEmpty(cartResult.CartId))
                //{
                (Application.Current as App).CartId = cartResult.CartId;
                await this.Navigation.PushAsync(new RegisterPage(deviceId.Value.ToString(), false));
                //}
                //else
                //{
                //    await this.DisplayAlert("SmartRetail", $"買い物を開始できません\n{cartResult.ErrorMessage}", "OK");

                //    // 取引開始できない場合はログインへ戻る
                //    await this.Navigation.PopAsync();
                //}

                //インジケータを隠す
                SetIndicator(false);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void btnCrash_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                Crashes.GenerateTestCrash();
            }catch(Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "CrashLog",$"DeviceId={(App.Current as App).DeviceId}"}
                });
            }
        }
    }
}