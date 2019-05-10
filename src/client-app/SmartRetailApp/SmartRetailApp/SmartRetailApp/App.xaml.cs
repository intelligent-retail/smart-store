using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using SmartRetailApp.Models;
using SmartRetailApp.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SmartRetailApp
{
    public partial class App : Application
    {
        public string CartId { get; set; }
        public string BoxId { get; set; }

        // 入店時間
        public DateTime EnterDate { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
#if DEBUG
            AppCenter.LogLevel = LogLevel.Verbose; // Before AppCenter.Start
#endif
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += this.Push_PushNotificationReceived;
            }

            AppCenter.Start($"android={Constant.AppCenterKeyAndroid};" +
                            "uwp={Your UWP App secret here};" +
                            $"ios={Constant.AppCenterKeyiOS}",
                typeof(Push));
        }

        private async void Push_PushNotificationReceived(object sender, PushNotificationReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
            {
                return;
            }

            if (e.CustomData != null && e.CustomData.ContainsKey("action"))
            {
                var action = e.CustomData["action"];
                switch (action)
                {
                    // カート情報を更新する
                    case "update_cart":
                        await PushCartPageAsync();
                        break;
                    // カート情報を更新する
                    case "receipt":
                        await PushReceiptPageAsync();
                        break;
                }
            }
        }

        /// <summary>
        /// カートを更新する
        /// </summary>
        async Task PushCartPageAsync()
        {
#if DEBUG
            await this.MainPage.DisplayAlert("SmartRetail", "カート情報を更新します", "OK");
#endif

            var nav = this.MainPage as NavigationPage;
            var deviceId = await AppCenter.GetInstallIdAsync();

            // すでにRegisterPageを開いている
            if (nav.CurrentPage is RegisterPage)
            {
                var cartPage = nav.CurrentPage as RegisterPage;
                await cartPage.UpdateCart();
            }
            else
            {
                await nav.Navigation.PushAsync(new RegisterPage(deviceId.ToString(), true));
            }
        }

        /// <summary>
        /// レシートを表示する
        /// </summary>
        async Task PushReceiptPageAsync()
        {

#if DEBUG
            await this.MainPage.DisplayAlert("SmartRetail", "レシートを表示します", "OK");
#endif

            var nav = this.MainPage as NavigationPage;

            // すでにRegisterPageを開いている
            if (nav.CurrentPage is ThankPage)
            {
                var thankPage = nav.CurrentPage as ThankPage;
                await thankPage.UpdateReceipt();
            }
            else
            {
                await nav.Navigation.PushAsync(new ThankPage());
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
