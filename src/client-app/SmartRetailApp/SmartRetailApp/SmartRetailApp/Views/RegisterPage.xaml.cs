#define MOCK
using SmartRetailApp.Models;
using SmartRetailApp.Services;
using SmartRetailApp.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SmartRetailApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        private string CartId => (Application.Current as App).CartId;

        public RegisterPage(string deviceId, bool isUpdateCart)
        {
            InitializeComponent();

            this.DeviceId.IsVisible = false;
            this.DeviceId.Text = "";
            this.DeviceId.IsVisible = true;

            // BOX名を表示
            lblBoxName.Text = (Application.Current as App).BoxId;

            // 入店時間
            lblEnterDate.Text = DateTime.Now.ToString();

            // 最終更新日
            (Application.Current as App).EnterDate = DateTime.Now;
            lblUpdateDate.Text = (Application.Current as App).EnterDate.ToString();

            this.lblTotalQuantities.Text = "0";
            this.lblTaxAmount.Text = "0";
            this.lblTotalAmount.Text = "0";

            // 開始はListが空なので表示しない
            SetListViewVisible(false);

            // カートを更新するかどうか
            if (isUpdateCart)
            {
                UpdateCart();
            }
        }

        void SetListViewVisible(bool isVivble)
        {
            listView.IsVisible = isVivble;
            lblEmpty.IsVisible = !isVivble;
        }

        /// <summary>
        /// APIを呼んでカートを更新する
        /// </summary>
        public void UpdateCart()
        {
            var service = new CartApiService();
            var cartId = this.CartId;

#if MOCK
            var cartStatus = Mock.CreateMockCart(1);
#else
            var cartStatus = await service.CartStatusAsync(cartId, CartApiService.CartAction.items);
#endif


            // 最終更新日
            lblUpdateDate.Text = DateTime.Now.ToString();

            // ユーザー名
            this.lblUserName.Text = cartStatus
                .User
                .UserName;

            // カートの商品をListViewにBinding

            if (cartStatus.Cart?.LineItems?.Length == 0)
            {
                // カートが空
                SetListViewVisible(false);
                return;
            }

            var cartList = cartStatus
                .Cart
                .LineItems
                .OrderByDescending(m => m.LineNo)
                .Select(m => new
                {
                    ItemName = m.ItemName,
                    UnitPrice = $"{m.UnitPrice:N0}",
                    Amount = $"{m.Amount:N0}",
                    Quantity = m.Quantity,
                    Image = m.ImageUrls !=null ? ImageSource.FromUri(new Uri(m.ImageUrls[0])) : null
                })
                .ToList();
            listView.BindingContext = cartList;

            // 先頭に移動する
            listView.ScrollTo(cartList[0], ScrollToPosition.MakeVisible, false);

            var totalQuantities = cartStatus.Cart.LineItems.Sum(m => m.Quantity);
            this.lblTotalQuantities.Text = $"{totalQuantities}";
            this.lblTaxAmount.Text = $"{cartStatus.Cart.Taxes[0].TaxAmount:N0}";
            this.lblTotalAmount.Text = $"{cartStatus.Cart.TotalAmount:N0}";

            // カートに商品があればListViewを表示する
            SetListViewVisible(true);
        }

        /// <summary>
        /// カートUIを更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClickUpdateCart(object sender, EventArgs e)
        {
            UpdateCart();
        }

        /// <summary>
        /// レシートページへ遷移する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClickPushReceiptPage(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new ThankPage());
        }
    }
}