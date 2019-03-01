using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using PosService.DataContracts.Items.Parameters;
using PosService.DataContracts.Items.Responses;
using PosService.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PosService.Models.Repositories.Http
{
    /// <summary>
    /// 商品リポジトリ
    /// </summary>
    public class ItemsRepository
    {
        /// <summary>HTTP クライアント</summary>
        private readonly HttpClient _client;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="client">HTTP クライアント</param>
        public ItemsRepository(HttpClient client) => this._client = client;

        /// <summary>
        /// 商品情報を取得します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="itemCodes">商品コード</param>
        /// <param name="log">ロガー</param>
        /// <returns>商品情報取得結果</returns>
        public async Task<GetItemsResponse> GetItemsAsync(
            string companyCode,
            string storeCode,
            List<string> itemCodes,
            ILogger log)
        {
            log.LogInformation($"Start GetItems[{string.Join(',', itemCodes.ToArray())}]");

            var param = new GetItemsParameter { ItemCodes = itemCodes };
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                Settings.Instance.ItemMasterUri.Replace("{company-code}", companyCode).Replace("{store-code}", storeCode));

            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add(Settings.Instance.FunctionsApiKeyHeader, Settings.Instance.ItemMasterApiKey);
            request.Content = new ObjectContent<GetItemsParameter>(param, new JsonMediaTypeFormatter());

            var start = DateTime.Now;
            var response = await this._client.SendAsync(request);
            var end = DateTime.Now;
            var elapsed = $"({(end - start).TotalMilliseconds} msec)";

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var res = JsonConvert.DeserializeObject<GetItemsResponse>(await response.Content.ReadAsStringAsync());
                var info = string.Join(',', res.Items.Select(i => $"[{i.ItemCode}:{i.Description} \\{i.UnitPrice}]").ToArray());
                log.LogInformation($"Succeeded GetItems{info} {elapsed}");
                return res;
            }
            else
            {
                log.LogWarning($"Items not found {elapsed}");
                return null;
            }
        }
    }
}
