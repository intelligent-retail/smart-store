using Microsoft.AppCenter;
using Microsoft.AppCenter.Auth;
using SmartRetailApp.Models;
using SmartRetailApp.Services;
using System;
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

            btnLoginLogout.Clicked += async (sender, e) =>
            {
                if (btnLoginLogout.Text == "ログアウト")
                {
                    await SignOut();
                }
                else
                {
                    var app = Application.Current as App;
                    if (await app.SignInAsync() != true)
                    {
                        await DisplayAlert("ログインできませんでした", app.AuthErrorMessage,"OK");
                    }
                    else
                    {
                        await DisplayAlert("ログインしました", $"", "OK");
                        btnLoginLogout.Text = "ログアウト";
                    }
                }
            };
        }

        async Task SignOut()
        {
            var app = Application.Current as App;
            app.SignOut();

            btnLoginLogout.Text = "ログイン";

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
                if (cartResult != null && /*cartResult.IsSuccess*/ !string.IsNullOrEmpty(cartResult.CartId))
                {
                    (Application.Current as App).CartId = cartResult.CartId;
                    await this.Navigation.PushAsync(new RegisterPage(deviceId.Value.ToString(), false));
                }
                else
                {
                    await this.DisplayAlert("SmartRetail", $"買い物を開始できません\n{cartResult.ErrorMessage}", "OK");

                    // 取引開始できない場合はログインへ戻る
                    await this.Navigation.PopAsync();
                }

                //インジケータを隠す
                SetIndicator(false);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}