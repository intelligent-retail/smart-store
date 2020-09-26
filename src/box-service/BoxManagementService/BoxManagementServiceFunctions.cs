using BoxManagementService.DataCotracts.Parameters;
using BoxManagementService.DataCotracts.Responses;
using BoxManagementService.Models.Repositories;
using BoxManagementService.Models.Repositories.CosmosDB;
using BoxManagementService.Models.Repositories.Http;
using BoxManagementService.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace BoxManagementService
{
    public static class BoxManagementServiceFunctions
    {
        static BoxManagementServiceFunctions()
        {
        }

        /// <summary>
        /// カート作成
        /// </summary>
        /// <param name="req">リクエスト</param>
        /// <param name="client">CosmosDBクライアント</param>
        /// <param name="log">ログ</param>
        /// <returns>結果</returns>
        [FunctionName("CreateCart")]
        public static async Task<IActionResult> CreateCart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/carts")] HttpRequest req,
            [CosmosDB(
                databaseName: "smartretailboxmanagement",
                collectionName: "Terminals",
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            var parameter = await HttpRequestUtility.GetRequestParameterAsync<CreateCartParameter>(req);

            var userRepo = new UserRepository(StaticHttpClient.Instance);
            var user = await userRepo.AuthenticateUserAsync();
            if (!user.result.IsSuccess)
            {
                return new UnauthorizedResult();
            }

            var terminalRepo = new TerminalRepository(client);
            var terminal = await terminalRepo.FindByBoxIdAsync(parameter.BoxId);
            if (!terminal.result.IsSuccess)
            {
                return ActionResultFactory.CreateError<CreateCartResponse>(terminal.result);
            }

            var cartRepo = new CartRepository(new CartServiceApi(StaticHttpClient.Instance));
            var cart = await cartRepo.CreateCartAsync(terminal.info.CompanyCode, terminal.info.StoreCode, terminal.info.TerminalNo, user.info.UserId, user.info.UserName);
            if (!cart.result.IsSuccess)
            {
                return ActionResultFactory.CreateError<CreateCartResponse>(cart.result);
            }

            var boxRepo = new BoxRepository(client);
            var updResult = await boxRepo.SetCartIdAsync(parameter.BoxId, parameter.DeviceId, user.info.UserId, cart.info.CartId);
            if (!updResult.IsSuccess)
            {
                return ActionResultFactory.CreateError<CreateCartResponse>(updResult);
            }

            await BoxUtility.RequestUnLockAsync(terminal.info.BoxId, terminal.info.BoxDeviceId);

            return new CreatedResult(
                string.Empty,
                new CreateCartResponse
                {
                    CartId = cart.info.CartId
                });
        }

        /// <summary>
        /// カート情報取得（明細）
        /// </summary>
        /// <param name="req">リクエスト</param>
        /// <param name="cartId">カートID</param>
        /// <param name="log">ログ</param>
        /// <returns>カート情報</returns>
        [FunctionName("GetCart")]
        public static async Task<IActionResult> GetCart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/carts/{cartId}/Items")] HttpRequest req,
            string cartId,
            ILogger log)
        {
            var cartRepo = new CartRepository(new CartServiceApi(StaticHttpClient.Instance));
            var cart = await cartRepo.GetCartAsync(cartId);

            return cart.result.IsSuccess
                ? new ObjectResult(cart.info)
                : ActionResultFactory.CreateError(cart.result, cart.info);
        }

        /// <summary>
        /// カート情報取得（精算）
        /// </summary>
        /// <param name="req">リクエスト</param>
        /// <param name="cartId">カートID</param>
        /// <param name="log">ログ</param>
        /// <returns>カート情報</returns>
        [FunctionName("GetBill")]
        public static async Task<IActionResult> GetBill(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/carts/{cartId}/bill")] HttpRequest req,
            string cartId,
            ILogger logger)
        {
            var cartRepo = new CartRepository(new CartServiceApi(StaticHttpClient.Instance));
            var cart = await cartRepo.GetCartAsync(cartId);

            return cart.result.IsSuccess
                ? new ObjectResult(cart.info)
                : ActionResultFactory.CreateError(cart.result, cart.info);
        }

        /// <summary>
        /// 取引開始時の在庫更新の関数です。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        [FunctionName("UpdateOpeningStocks")]
        public static async Task<BoxResponse> UpdateOpeningStocks(
            [ActivityTrigger] IList<UpdateStocksParameter> parameters,
            [CosmosDB(
                databaseName: null,
                collectionName: null,
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            (var response, var repoResult) = await UpdateOpeningStocksAsync(parameters, client, log);

            return response;
        }

        /// <summary>
        /// 取引終了時の在庫更新の関数です。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        [FunctionName("UpdateClosingStocks")]
        public static async Task<BoxResponse> UpdateClosingStocks(
            [ActivityTrigger] IList<UpdateStocksParameter> parameters,
            [CosmosDB(
                databaseName: null,
                collectionName: null,
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            (var response, var repoResult) = await UpdateClosingStocksAsync(parameters, client, log);

            return response;
        }

        /// <summary>
        /// 商品更新の関数です。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        [FunctionName("UpdateItems")]
        public static async Task<BoxResponse> UpdateItems(
            [ActivityTrigger] IList<UpdateItemsParameter> parameters,
            [CosmosDB(
                databaseName: null,
                collectionName: null,
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            (var response, var repoResult) = await UpdateItemsAsync(parameters, client, log);

            return response;
        }

        /// <summary>
        /// 取引開始時の在庫更新を行います。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        /// <remarks>BOX で付与されたタイムスタンプ順に処理されます。</remarks>
        private static async Task<(BoxResponse, RepositoryResult)> UpdateOpeningStocksAsync(
            IList<UpdateStocksParameter> parameters,
            DocumentClient client,
            ILogger log)
        {
            var stockRepo = new StockRepository(client);

            parameters = parameters
                .OrderBy(arg => arg.Timestamp)
                .ToArray();

            foreach (var parameter in parameters)
            {
                // 在庫を設定します。
                var setStocksResult = await stockRepo.SetStocksAsync(
                    parameter.BoxId,
                    "0",
                    parameter.Items.Select(arg => (arg.SkuCode, arg.Quantity)).ToArray());
                if (!setStocksResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(setStocksResult), setStocksResult);
                }
            }

            return (new BoxResponse(), null);
        }

        /// <summary>
        /// 取引終了時の在庫更新を行います。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        /// <remarks>BOX で付与されたタイムスタンプ順に処理されます。</remarks>
        private static async Task<(BoxResponse, RepositoryResult)> UpdateClosingStocksAsync(
            IList<UpdateStocksParameter> parameters,
            DocumentClient client,
            ILogger log)
        {
            var terminalRepo = new TerminalRepository(client);
            var boxRepo = new BoxRepository(client);
            var skuRepo = new SkuRepository(client);
            var stockRepo = new StockRepository(client);
            var cartRepo = new CartRepository(new CartServiceApi(StaticHttpClient.Instance));

            parameters = parameters
                .OrderBy(arg => arg.Timestamp)
                .ToArray();

            foreach (var parameter in parameters)
            {
                // 在庫を設定します。
                var setStocksResult = await stockRepo.SetStocksAsync(
                    parameter.BoxId,
                    "1",
                    parameter.Items.Select(arg => (arg.SkuCode, arg.Quantity)).ToArray());
                if (!setStocksResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(setStocksResult), setStocksResult);
                }

                // ターミナルを取得します。
                (var terminalResult, var terminal) = await terminalRepo.FindByBoxIdAsync(parameter.BoxId);
                if (!terminalResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(terminalResult), terminalResult);
                }

                // BOX を取得します。
                (var boxResult, var box) = await boxRepo.FindByBoxIdAsync(parameter.BoxId);
                if (!boxResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(boxResult), boxResult);
                }

                // 取引開始時と比較して、在庫数に差異がある BOX 商品を取得します。
                (var stockDifferencesResult, var stockReferrences) = await stockRepo.GetStockDifferencesAsync(parameter.BoxId);
                if (!stockDifferencesResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(stockDifferencesResult), stockDifferencesResult);
                }

                // BOX 商品を POS 商品に変換し、
                // 取引開始時と比較して、在庫数に差異がある POS 商品を抽出します。
                var items = stockReferrences
                    .Select(arg => (arg.SkuCode, arg.Quantity))
                    .ToArray();

                (var posItemsResult, var posItems) = await BoxItemsToPosItemsAsync(terminal.CompanyCode, terminal.StoreCode, items, skuRepo);
                if (!posItemsResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(posItemsResult), posItemsResult);
                }

                var changedPosItems = posItems
                    .Where(arg => arg.Quantity != 0)
                    .Select(arg => (arg.ItemCode, 0 < arg.Quantity ? 1 : 2, Math.Abs(arg.Quantity)))
                    .ToArray();

                if (changedPosItems.Any())
                {
                    // 取引を確定します。
                    var subtotalResult = await cartRepo.SubtotalAsync(box.CartId, changedPosItems);
                    if (!subtotalResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(subtotalResult), subtotalResult);
                    }

                    var createPaymentResult = await cartRepo.CreatePaymentsAsync(box.CartId, new[] { ("01", 0M) });
                    if (!createPaymentResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(createPaymentResult), createPaymentResult);
                    }

                    var billResult = await cartRepo.BillAsync(box.CartId);
                    if (!billResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(billResult), billResult);
                    }
                }
                else
                {
                    // 取引を取り消します。
                    var deleteCartResult = await cartRepo.DeleteCartAsync(box.CartId);
                    if (!deleteCartResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(deleteCartResult), deleteCartResult);
                    }
                }

                // デバイスに通知します。
                await new NotificationUtility(log).PushClosedCartNotificationAsync(box.DeviceId);
            }

            return (new BoxResponse(), null);
        }

        /// <summary>
        /// 商品更新を行います。
        /// </summary>
        /// <param name="parameters">パラメーター。</param>
        /// <param name="client">Cosmos DB クライアント。</param>
        /// <param name="log">ロガー。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理の結果が含まれます。</returns>
        /// <remarks>同一の BOX に対する処理は、まとめて 1 回で行います。</remarks>
        private static async Task<(BoxResponse, RepositoryResult)> UpdateItemsAsync(
            IList<UpdateItemsParameter> parameters,
            DocumentClient client,
            ILogger log)
        {
            var terminalRepo = new TerminalRepository(client);
            var boxRepo = new BoxRepository(client);
            var skuRepo = new SkuRepository(client);
            var cartRepo = new CartRepository(new CartServiceApi(StaticHttpClient.Instance));

            var parameterGroups = parameters
                .GroupBy(arg => arg.BoxId)
                .ToArray();

            foreach (var parameterGroup in parameterGroups)
            {
                var boxId = parameterGroup.Key;

                // ターミナルを取得します。
                (var terminalResult, var terminal) = await terminalRepo.FindByBoxIdAsync(boxId);
                if (!terminalResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(terminalResult), terminalResult);
                }

                // BOX を取得します。
                (var boxResult, var box) = await boxRepo.FindByBoxIdAsync(boxId);
                if (!boxResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(boxResult), boxResult);
                }

                // BOX 商品を POS 商品に変換します。
                var items = parameterGroup
                    .SelectMany(arg => arg.Items)
                    .Select(arg => (arg.SkuCode, arg.Quantity))
                    .ToArray();

                (var posItemsResult, var posItems) = await BoxItemsToPosItemsAsync(terminal.CompanyCode, terminal.StoreCode, items, skuRepo);
                if (!posItemsResult.IsSuccess)
                {
                    return (BoxResponseFactory.CreateError<BoxResponse>(posItemsResult), posItemsResult);
                }

                // カートに POS 商品を追加します。全ての商品をまとめて一度で行います。
                var increasedPosItems = posItems
                    .Where(arg => 0 < arg.Quantity)
                    .ToArray();

                if (increasedPosItems.Any())
                {
                    var addItemsResult = await cartRepo.AddItemsAsync(box.CartId, increasedPosItems);
                    if (!addItemsResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(addItemsResult), addItemsResult);
                    }
                }

                // カートから POS 商品を削除します。商品ごとに行います。
                var decreasedPosItems = posItems
                    .Where(arg => arg.Quantity < 0)
                    .Select(arg => (arg.ItemCode, Math.Abs(arg.Quantity)))
                    .ToArray();

                foreach (var (itemCode, quantity) in decreasedPosItems)
                {
                    var removeItemResult = await cartRepo.RemoveItemAsync(box.CartId, itemCode, quantity);
                    if (!removeItemResult.IsSuccess)
                    {
                        return (BoxResponseFactory.CreateError<BoxResponse>(removeItemResult), removeItemResult);
                    }
                }

                // デバイスに通知します。
                await new NotificationUtility(log).PushUpdatedCartNotificationAsync(box.DeviceId);
            }

            return (new BoxResponse(), null);
        }

        /// <summary>
        /// BOX 商品を POS 商品に変換します。
        /// 同一の POS 商品ごとに数量を合計します。
        /// </summary>
        /// <param name="companyCode">法人コード。</param>
        /// <param name="storeCode">店舗コード。</param>
        /// <param name="items">BOX 商品。</param>
        /// <param name="skuRepo">SKU リポジトリ。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果が含まれます。</returns>
        private static async Task<(RepositoryResult Result, IList<(string ItemCode, int Quantity)> PosItems)> BoxItemsToPosItemsAsync(
            string companyCode, string storeCode, IEnumerable<(string SkuCode, int Quantity)> items, SkuRepository skuRepo)
        {
            // SKU コードごとにグループ化します。
            var itemGroups = items
                .GroupBy(arg => arg.SkuCode)
                .ToArray();

            // SKU コードを商品コードに変換し、数量を合計します。
            // BOX 商品の数量減は、POS 商品の数量増を表すので、合計の際、数量の符号は反転させます。
            var posItems = new Dictionary<string, int>();

            foreach (var itemGroup in itemGroups)
            {
                var skuCode = itemGroup.Key;

                (var skuResult, var sku) = await skuRepo.FindBySkuCodeAsync(companyCode, storeCode, skuCode);
                if (!skuResult.IsSuccess)
                {
                    return (skuResult, null);
                }

                if (!posItems.ContainsKey(sku.ItemCode))
                {
                    posItems.Add(sku.ItemCode, 0);
                }

                posItems[sku.ItemCode] += itemGroup.Sum(arg => -arg.Quantity);
            }

            return (RepositoryResult.OK, posItems.Select(arg => (arg.Key, arg.Value)).ToArray());
        }
    }
}
