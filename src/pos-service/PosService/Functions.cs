using System;
using System.Net;
using System.Threading.Tasks;
using PosService.DataContracts.Pos.Parameters;
using PosService.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PosService
{
    /// <summary>
    /// Azure Functions でホストする POS Functions
    /// </summary>
    public static class Functions
    {
        /// <summary>Functions のルートとなる共通 URI</summary>
        private const string FunctionsRoute = "v1/carts";

        /// <summary>Cosmos DB 接続文字列の設定名称</summary>
        private const string CosmosDBConnectionStringSetting = "CosmosDbConnectionString";

        /// <summary>
        /// カートを作成します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="log">ロガー</param>
        /// <returns>
        /// 作成に成功した時
        /// 　201:Created
        /// 　　　Content に CreateCartResponse が設定されます
        /// 作成に失敗した時
        /// 　パラメーター誤り
        /// 　　400:BadRequest
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(CreateCart))]
        public static async Task<IActionResult> CreateCart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = FunctionsRoute)] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            ILogger log) =>
            await DoAsync(log, nameof(CreateCart), HttpStatusCode.Created, async () =>
                {
                    var param = await HttpRequestUtility.GetRequestParameterAsync<CreateCartParameter>(req);
                    if (param == null)
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }

                    return await new PosLogic(log).CreateCartAsync(param, client);
                });

        /// <summary>
        /// カートを取得します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
        /// <returns>
        /// 取得に成功した時
        /// 　200:OK
        /// 　　　Content に GetCartResponse が設定されます
        /// 取得に失敗した時
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(GetCart))]
        public static async Task<IActionResult> GetCart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = FunctionsRoute + "/{cartId}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(GetCart), HttpStatusCode.OK, async () => await new PosLogic(log).GetCartAsync(client, cartId));

        /// <summary>
        /// カートに商品を追加します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
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
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(AddItems))]
        public static async Task<IActionResult> AddItems(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = FunctionsRoute + "/{cartId}/items")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(AddItems), HttpStatusCode.OK, async () =>
                {
                    var param = await HttpRequestUtility.GetRequestParameterAsync<AddItemsParameter>(req);
                    if (param == null)
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }

                    return await new PosLogic(log).AddItemsToCartAsync(param, client, cartId);
                });

        /// <summary>
        /// カートから商品を削除します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="itemCode">商品コード</param>
        /// <param name="log">ロガー</param>
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
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(DeleteItem))]
        public static async Task<IActionResult> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = FunctionsRoute + "/{cartId}/items/{itemCode}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            string itemCode,
            ILogger log) =>
            await DoAsync(log, nameof(DeleteItem), HttpStatusCode.OK, async () =>
                {
                    if (req.Query.ContainsKey("quantity") && int.TryParse(req.Query["quantity"], out int quantity))
                    {
                        return await new PosLogic(log).DeleteItemFromCartAsync(client, cartId, itemCode, quantity);
                    }
                    else
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }
                });

        /// <summary>
        /// カートを小計します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
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
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(Subtotal))]
        public static async Task<IActionResult> Subtotal(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = FunctionsRoute + "/{cartId}/subtotal")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(Subtotal), HttpStatusCode.OK, async () =>
                {
                    var param = await HttpRequestUtility.GetRequestParameterAsync<SubtotalParameter>(req);
                    if (param == null)
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }

                    return await new PosLogic(log).SubtotalCartAsync(param, client, cartId);
                });

        /// <summary>
        /// カートに支払を追加します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
        /// <returns>
        /// 追加に成功した時
        /// 　200:OK
        /// 追加に失敗した時
        /// 　パラメーター誤り または
        /// 　カート状態が処理を受け付けることができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(AddPayments))]
        public static async Task<IActionResult> AddPayments(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = FunctionsRoute + "/{cartId}/payments")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(AddPayments), HttpStatusCode.OK, async () =>
                {
                    var param = await HttpRequestUtility.GetRequestParameterAsync<AddPaymentsParameter>(req);
                    if (param == null)
                    {
                        return new BadRequestObjectResult(Errors.ParameterError);
                    }

                    return await new PosLogic(log).AddPaymentsToCartAsync(param, client, cartId);
                });

        /// <summary>
        /// カートの取引を確定します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
        /// <returns>
        /// 取引確定に成功した時
        /// 　200:OK
        /// 取引確定に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(Bill))]
        public static async Task<IActionResult> Bill(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = FunctionsRoute + "/{cartId}/bill")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(Bill), HttpStatusCode.OK, async () => await new PosLogic(log).BillCartAsync(client, cartId));

        /// <summary>
        /// カートの取引を中止します
        /// </summary>
        /// <param name="req">HttpRequest オブジェクト</param>
        /// <param name="client">Cosmos DB クライアント</param>
        /// <param name="cartId">カート ID</param>
        /// <param name="log">ロガー</param>
        /// <returns>
        /// 取引中止に成功した時
        /// 　200:OK
        /// 取引中止に失敗した時
        /// 　カート状態が処理を受け付けることができない または
        /// 　在庫情報の更新ができない
        /// 　　400:BadRequest
        /// 　指定したカート ID に該当するカートが存在しない
        /// 　　404:NotFound
        /// 　例外発生時
        /// 　　500:InternalServerError
        /// </returns>
        [FunctionName(nameof(CancelCart))]
        public static async Task<IActionResult> CancelCart(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = FunctionsRoute + "/{cartId}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = CosmosDBConnectionStringSetting)] DocumentClient client,
            string cartId,
            ILogger log) =>
            await DoAsync(log, nameof(CancelCart), HttpStatusCode.OK, async () => await new PosLogic(log).CancelCartAsync(client, cartId));

        /// <summary>
        /// API に事前・事後処理を付加します
        /// </summary>
        /// <param name="log">ロガー</param>
        /// <param name="name">API 名</param>
        /// <param name="expectCode">期待する正常終了コード</param>
        /// <param name="func">API ロジック</param>
        /// <returns>API の結果</returns>
        private static async Task<IActionResult> DoAsync(
            ILogger log,
            string name,
            HttpStatusCode expectCode,
            Func<Task<ObjectResult>> func)
        {
            var start = DateTimeUtility.GetApplicationTimeNow();
            log.LogInformation($"Start {name}:{start.ToString("yyyy/MM/dd HH:mm:ss.fff")}");

            ObjectResult result = null;
            try
            {
                result = await func();
                return result;
            }
            finally
            {
                var end = DateTimeUtility.GetApplicationTimeNow();

                string commonMessage = $"End {name}:{end.ToString("yyyy/MM/dd HH:mm:ss.fff")} ({(end - start).TotalMilliseconds} msec)";
                if (result == null)
                {
                    log.LogError(commonMessage + " result == null");
                }
                else
                {
                    if (!(result.Value is string val))
                    {
                        val = result.Value?.ToString() ?? "null";
                    }

                    if ((result.StatusCode ?? 0) == (int)expectCode)
                    {
                        log.LogInformation(commonMessage + $" {result.StatusCode}:{val}");
                    }
                    else
                    {
                        log.LogError(commonMessage + $" {result.StatusCode}:{val}");
                    }
                }
            }
        }
    }
}
