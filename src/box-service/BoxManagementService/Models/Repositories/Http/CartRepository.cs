using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.Http
{
    public class CartRepository : HttpRepositoryBase
    {
        private readonly CartServiceApi _api;

        public CartRepository(CartServiceApi api)
        {
            this._api = api;
        }

        /// <summary>
        /// カートを作成します。
        /// </summary>
        /// <param name="companyCode">法人コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <param name="userId">ユーザーID</param>
        /// <param name="userName">ユーザー名</param>
        /// <returns>カート情報</returns>
        public async Task<(RepositoryResult result, CartInformation info)> CreateCartAsync(string companyCode, string storeCode, int terminalNo, string userId, string userName)
        {
            var (result, body) = await this.PostAsync(
                this._api.CartsApi,
                new
                {
                    companyCode,
                    storeCode,
                    terminalNo,
                    userId,
                    userName
                });

            return (result, new CartInformation() { CartId = body.cartId });
        }

        /// <summary>
        /// カート情報を取得します。
        /// </summary>
        /// <param name="cartId">カートID</param>
        /// <returns>カート情報</returns>
        public async Task<(RepositoryResult result, dynamic info)> GetCartAsync(string cartId)
        {
            // カート情報要求はPOS側から取得した内容をそのまま返すため、dynamicでデシリアライズしたレスポンスボディをそのまま返している。
            return await this.GetAsync(this._api.GetCartApi(cartId));
        }

        /// <summary>
        /// カートを削除します。これにより、取引中止となります。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> DeleteCartAsync(string cartId)
        {
            var api = this._api.GetCartApi(cartId);

            var (result, body) = await this.DeleteAsync(api);

            return result;
        }

        /// <summary>
        /// カートに商品を追加します。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <param name="items">追加する商品。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> AddItemsAsync(string cartId, IEnumerable<(string itemCode, int quantity)> items)
        {
            var api = this._api.GetCartItemsApi(cartId);

            var content = new
            {
                items = items.Select(arg => new { arg.itemCode, arg.quantity }).ToArray()
            };

            var (result, body) = await this.PostAsync(api, content);

            return result;
        }

        /// <summary>
        /// カートから商品を削除します。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <param name="itemCode">削除する商品のコード。</param>
        /// <param name="quantity">削除する商品の数量。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> RemoveItemAsync(string cartId, string itemCode, int quantity)
        {
            var api = this._api.GetCartItemApi(cartId, itemCode);

            var query = new { quantity };

            var (result, body) = await this.DeleteAsync(api, query: query);

            return result;
        }

        /// <summary>
        /// 小計します。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <param name="items">取引開始時から数量に増減があった商品。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> SubtotalAsync(string cartId, IEnumerable<(string itemCode, int addType, int quantity)> items)
        {
            var api = this._api.GetCartSubtotalApi(cartId);

            var content = new
            {
                items = items.Select(arg => new { arg.itemCode, arg.addType, arg.quantity }).ToArray()
            };

            var (result, body) = await this.PostAsync(api, content);

            return result;
        }

        /// <summary>
        /// 支払いを登録します。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <param name="payments">支払い情報。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> CreatePaymentsAsync(string cartId, IEnumerable<(string paymentCode, decimal amount)> payments)
        {
            var api = this._api.GetCartPaymentsApi(cartId);

            var content = new
            {
                payments = payments.Select(arg => new { arg.paymentCode, arg.amount }).ToArray()
            };

            var (result, body) = await this.PostAsync(api, content);

            return result;
        }

        /// <summary>
        /// 取引を確定します。
        /// </summary>
        /// <param name="cartId">カート ID。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        public async Task<RepositoryResult> BillAsync(string cartId)
        {
            var api = this._api.GetCartBillApi(cartId);

            var (result, body) = await this.PostAsync(api);

            return result;
        }
    }
}
