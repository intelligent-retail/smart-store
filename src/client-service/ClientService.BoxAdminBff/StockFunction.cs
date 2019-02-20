using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ClientService.BoxAdminBff.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace ClientService.BoxAdminBff
{
    public static class StockFunction
    {
        [FunctionName(nameof(StocksByTerminal))]
        public static async Task<IActionResult> StocksByTerminal(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/terminal/{terminalCode}/stocks")] HttpRequest req,
            string companyCode,
            string storeCode,
            string terminalCode,
            ILogger log)
        {
            var stockQuery = await _httpClient.PostAsync(string.Format(_stockByTerminalApi, companyCode, storeCode, terminalCode), new StringContent(""));

            var stockQueryResponse = await stockQuery.Content.ReadAsAsync<QueryResponse<QueryResponse.QueryByTerminalItem>>();

            var itemMaster = await _httpClient.PostAsJsonAsync(string.Format(_itemMasterApi, companyCode, storeCode), new
            {
                itemCodes = stockQueryResponse.Items
                                              .Select(x => x.ItemCode)
                                              .ToArray()
            });

            var itemMasterResponse = await itemMaster.Content.ReadAsAsync<ItemResponse>();

            var response = new StockResponse
            {
                Items = stockQueryResponse.Items.Select(x =>
                                          {
                                              var item = itemMasterResponse.Items.FirstOrDefault(xs => xs.ItemCode == x.ItemCode);

                                              return new StockItemResponse
                                              {
                                                  ItemCode = x.ItemCode,
                                                  ItemName = item?.Description,
                                                  ManufacturerName = item?.ManufacturerCode,
                                                  Quantity = x.Quantity
                                              };
                                          })
                                          .ToArray()
            };

            return new OkObjectResult(response);
        }

        [FunctionName(nameof(StocksByStore))]
        public static async Task<IActionResult> StocksByStore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/stocks")] HttpRequest req,
            string companyCode,
            string storeCode,
            ILogger log)
        {
            var stockQuery = await _httpClient.PostAsync(string.Format(_stockByStoreApi, companyCode, storeCode), new StringContent(""));

            var stockQueryResponse = await stockQuery.Content.ReadAsAsync<QueryResponse<QueryResponse.QueryByStoreItem>>();

            var itemMaster = await _httpClient.PostAsJsonAsync(string.Format(_itemMasterApi, companyCode, storeCode), new
            {
                itemCodes = stockQueryResponse.Items
                                              .Select(x => x.ItemCode)
                                              .ToArray()
            });

            var itemMasterResponse = await itemMaster.Content.ReadAsAsync<ItemResponse>();

            var res = new MonitorResponse
            {
                CompanyCode = companyCode,
                Store = new MonitorStoreResponse
                {
                    StoreCode = storeCode,
                    Terminals = stockQueryResponse.Items
                                                  .ToLookup(x => x.TerminalCode)
                                                  .Select(x => new MonitorTerminalResponse
                                                  {
                                                      TerminalCode = x.Key,
                                                      Items = x.Select(xs =>
                                                               {
                                                                   var item = itemMasterResponse.Items.FirstOrDefault(xss => xss.ItemCode == xs.ItemCode);

                                                                   return new StockItemResponse
                                                                   {
                                                                       ItemCode = xs.ItemCode,
                                                                       ItemName = item?.Description,
                                                                       ManufacturerName = item?.ManufacturerCode,
                                                                       Quantity = xs.Quantity
                                                                   };
                                                               })
                                                               .ToArray()

                                                  })
                                                  .ToArray()
                }
            };

            return new OkObjectResult(res);
        }

        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly string _stockByStoreApi = Environment.GetEnvironmentVariable("StockByStoreApi", EnvironmentVariableTarget.Process);
        private static readonly string _stockByTerminalApi = Environment.GetEnvironmentVariable("StockByTerminalApi", EnvironmentVariableTarget.Process);
        private static readonly string _itemMasterApi = Environment.GetEnvironmentVariable("ItemMasterApi", EnvironmentVariableTarget.Process);
    }

    public class StockResponse
    {
        [JsonProperty("items")]
        public StockItemResponse[] Items { get; set; }
    }

    public class StockItemResponse
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("itemName")]
        public string ItemName { get; set; }

        [JsonProperty("manufacturerName")]
        public string ManufacturerName { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class MonitorResponse
    {
        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }

        [JsonProperty("store")]
        public MonitorStoreResponse Store { get; set; }
    }

    public class MonitorStoreResponse
    {
        [JsonProperty("storeCode")]
        public string StoreCode { get; set; }

        [JsonProperty("terminals")]
        public MonitorTerminalResponse[] Terminals { get; set; }
    }

    public class MonitorTerminalResponse
    {
        [JsonProperty("terminalCode")]
        public string TerminalCode { get; set; }

        [JsonProperty("items")]
        public StockItemResponse[] Items { get; set; }
    }
}
