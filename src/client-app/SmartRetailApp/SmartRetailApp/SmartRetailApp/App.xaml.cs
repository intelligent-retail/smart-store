using Microsoft.AppCenter;
using Microsoft.Identity.Client;
using SmartRetailApp.Models;
using SmartRetailApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SmartRetailApp
{
    public partial class App : Application
    {
        public static IPublicClientApplication AuthenticationClient { get; private set; }
        public static object UIParent { get; set; } = null;

        public string CartId { get; set; }
        public string BoxId { get; set; }
        public string AuthErrorMessage { get; set; }

        // 入店時間
        public DateTime EnterDate { get; set; }

        public App()
        {
            InitializeComponent();

            AuthenticationClient = PublicClientApplicationBuilder.Create(Constant.ClientId)
                .WithIosKeychainSecurityGroup(Constant.IosKeyChain)
                .WithB2CAuthority(Constant.AuthoritySignin)
                .WithRedirectUri($"msal{Constant.ClientId}://auth")
                .Build();

            MainPage = new NavigationPage(new LoginPage());
        }

        protected override async void OnStart()
        {
            AppCenter.Start($"android={Constant.AppCenterKeyAndroid};" +
                            "uwp={Your UWP App secret here};" +
                            $"ios={Constant.AppCenterKeyiOS}");
        }

        public async Task<AuthenticationResult> SignInAsync()
        {
            AuthenticationResult result=null;
            try
            {
                result = await App.AuthenticationClient
                    .AcquireTokenInteractive(Constant.Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .WithParentActivityOrWindow(App.UIParent)
                    .ExecuteAsync();

            }
            catch (MsalException ex)
            {
                // The user has forgotten their password.
                // https://docs.microsoft.com/en-us/azure/active-directory-b2c/error-codes
                if (ex.Message != null && ex.Message.Contains("AADB2C90118"))
                {
                    SignOut();
                }
                else if (ex.ErrorCode != "authentication_canceled")
                {
                    this.AuthErrorMessage = ex.Message;
                }
            }

            return result;
        }

        public async void SignOut()
        {
            IEnumerable<IAccount> accounts = await App.AuthenticationClient.GetAccountsAsync();

            while (accounts.Any())
            {
                await App.AuthenticationClient.RemoveAsync(accounts.First());
                accounts = await App.AuthenticationClient.GetAccountsAsync();
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
