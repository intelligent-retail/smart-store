using SmartRetailApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartRetailApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThankPage : ContentPage
    {
        private string CartId => (Application.Current as App).CartId;

        public ThankPage()
        {
            InitializeComponent();

            // BOX名を表示
            lblBoxName.Text = (Application.Current as App).BoxId;

            UpdateReceipt();
        }

        /// <summary>
        /// APIを呼んでカートを更新する
        /// </summary>
        public async Task UpdateReceipt()
        {
            var service = new CartApiService();
            var cartId = this.CartId;
            var cartStatus = await service.CartStatusAsync(cartId, CartApiService.CartAction.bill);

            // ユーザー名
            this.lblUserName.Text = cartStatus.User.UserName;

            // レシートをListViewにBinding
            var cartList = cartStatus
                .Cart
                .LineItems
                .Select(m => new
                {
                    ItemName = m.ItemName,
                    UnitPrice = $"{m.UnitPrice:N0}",
                    Num = m.Amount,
                    Quantity = m.Quantity,
                });
            listView.BindingContext = cartList;

            this.lblTaxAmount.Text = $"{cartStatus.Cart.Taxes[0].TaxAmount:N0}";
            this.lblTotalAmount.Text = $"{cartStatus.Cart.TotalAmount:N0}";
            var totalQuantities = cartStatus.Cart.LineItems.Sum(m => m.Quantity);
            this.lblTotalQuantities.Text = $"{totalQuantities}";

            // 入店時間
            lblEnterDate.Text = (Application.Current as App).EnterDate.ToString();
            // 最終更新日
            lblUpdateDate.Text = DateTime.Now.ToString();
        }

        private void ClickToStartPage(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}