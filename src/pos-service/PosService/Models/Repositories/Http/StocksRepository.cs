using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using PosService.Constants;
using PosService.DataContracts.Items.Parameters;
using PosService.Models.Documents;
using PosService.Utilities;
using Microsoft.Extensions.Logging;
using static PosService.DataContracts.Items.Parameters.UpdateStocksParameter;

namespace PosService.Models.Repositories.Http
{
    /// <summary>
    /// 在庫リポジトリ
    /// </summary>
    public class StocksRepository
    {
        /// <summary>HTTP クライアント</summary>
        private readonly HttpClient _client;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="client">HTTP クライアント</param>
        public StocksRepository(HttpClient client) => this._client = client;

        /// <summary>
        /// 在庫情報を更新します
        /// </summary>
        /// <param name="cart">カート Document</param>
        /// <param name="items">商品在庫</param>
        /// <param name="allocationType">引当種別</param>
        /// <returns>在庫情報更新結果</returns>
        public async Task<bool> UpdateStocksAsync(
            CartDocument cart,
            List<StockItem> items,
            StockAllocationType allocationType,
            ILogger log)
        {
            var info = string.Join(
                ',',
                items
                    .OrderBy(i => i.LineNo)
                    .Select(i => $"[{i.ItemCode} x {i.Quantity}]")
                    .ToArray());
            log.LogInformation($"Start UpdateStocks({allocationType.Name}) cart:{cart.CartId}{info}");

            // HACK : LocationCode は在庫システム等と要調整
            var param = new UpdateStocksParameter
            {
                TransactionId = cart.TransactionNo,
                TransactionDate = cart.GenerateDateTime.Date,
                TransactionType = allocationType.Code,
                LocationCode = "0001",
                CompanyCode = cart.CompanyCode,
                StoreCode = cart.StoreCode,
                TerminalCode = cart.TerminalNo,
                Items = items
            };

            var request = new HttpRequestMessage(HttpMethod.Post, Settings.Instance.StockUri);
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add(Settings.Instance.FunctionsApiKeyHeader, Settings.Instance.StockApiKey);
            request.Content = new ObjectContent<UpdateStocksParameter>(param, new JsonMediaTypeFormatter());

            var start = DateTime.Now;
            var response = await this._client.SendAsync(request);
            var end = DateTime.Now;
            var elapsed = $"({(end - start).TotalMilliseconds} msec)";

            // HACK : 400 bad request の種別に応じたエラーハンドリングは省略している
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                log.LogInformation($"Succeeded UpdateStocks {elapsed}");
                return true;
            }
            else
            {
                log.LogError($"Failed UpdateStocks {elapsed}");
                return false;
            }
        }
    }
}
