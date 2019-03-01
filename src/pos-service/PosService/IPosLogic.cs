using System.Threading.Tasks;
using PosService.DataContracts.Pos.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;

namespace PosService
{
    /// <summary>
    /// POS ロジックインターフェース
    /// </summary>
    public interface IPosLogic
    {
        /// <summary>
        /// カートを作成します
        /// </summary>
        /// <param name="param">カート作成パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <returns>
        /// 作成に成功した時
        /// 　201:Created
        /// 　　　Content に CreateCartResponse が設定されます
        /// 作成に失敗した時
        /// 　パラメーター誤り
        /// 　　400:BadRequest
        /// </returns>
        Task<ObjectResult> CreateCartAsync(CreateCartParameter param, DocumentClient client);

        /// <summary>
        /// カートを取得します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取得に成功した時
        /// 　200:OK
        /// 　　　Content に GetCartResponse が設定されます
        /// 取得に失敗した時
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> GetCartAsync(DocumentClient client, string cartId);

        /// <summary>
        /// カートに商品を追加します
        /// </summary>
        /// <param name="param">商品追加パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない または
        /// 　指定した商品が存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> AddItemsToCartAsync(AddItemsParameter param, DocumentClient client, string cartId);

        /// <summary>
        /// カートから商品を削除します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="itemCode">商品コード</param>
        /// <param name="quantity">削除する数量</param>
        /// <returns>
        /// 削除に成功した時
        /// 　200:OK
        /// 削除に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　カート内の商品数量を超える返品数量を指定した または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> DeleteItemFromCartAsync(DocumentClient client, string cartId, string itemCode, int quantity);

        /// <summary>
        /// カートを小計します
        /// </summary>
        /// <param name="param">小計パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 小計に成功した時
        /// 　200:OK
        /// 小計に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない または
        /// 　指定した商品が存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> SubtotalCartAsync(SubtotalParameter param, DocumentClient client, string cartId);

        /// <summary>
        /// カートに支払を追加します
        /// </summary>
        /// <param name="param">支払追加パラメーター</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> AddPaymentsToCartAsync(AddPaymentsParameter param, DocumentClient client, string cartId);

        /// <summary>
        /// カートの取引を確定します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取引確定に成功した時
        /// 　200:OK
        /// 取引確定に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> BillCartAsync(DocumentClient client, string cartId);

        /// <summary>
        /// カートの取引を中止します
        /// </summary>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <returns>
        /// 取引中止に成功した時
        /// 　200:OK
        /// 取引中止に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// </returns>
        Task<ObjectResult> CancelCartAsync(DocumentClient client, string cartId);
    }
}
