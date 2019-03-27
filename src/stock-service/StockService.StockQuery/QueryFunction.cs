using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace StockService.StockQuery
{
    public static class QueryFunction
    {
        [FunctionName(nameof(QueryByItem))]
        public static async Task<IActionResult> QueryByItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/terminal/{terminalCode}/item/{itemCode}/query")]
            HttpRequest req,
            string companyCode,
            string storeCode,
            string terminalCode,
            string itemCode,
            ILogger log)
        {
            using (var context = new StockDbContext())
            {
                var item = await context.Stocks
                                         .Where(x => x.CompanyCode == companyCode && x.StoreCode == storeCode &&
                                                     x.TerminalCode == terminalCode && x.ItemCode == itemCode && x.TransactionType != TransactionType.引当)
                                         .GroupBy(x => x.ItemCode)
                                         .Select(x => new QueryByItem { ItemCode = x.Key, Quantity = x.Sum(xs => xs.Quantity) })
                                         .FirstOrDefaultAsync();

                var response = item ?? new QueryByItem { ItemCode = itemCode, Quantity = 0 };

                return new OkObjectResult(response);
            }
        }

        [FunctionName(nameof(QueryByTerminal))]
        public static async Task<IActionResult> QueryByTerminal(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/terminal/{terminalCode}/query")]
            HttpRequest req,
            string companyCode,
            string storeCode,
            string terminalCode,
            ILogger log)
        {
            using (var context = new StockDbContext())
            {
                var items = await context.Stocks
                                         .Where(x => x.CompanyCode == companyCode && x.StoreCode == storeCode &&
                                                     x.TerminalCode == terminalCode && x.TransactionType != TransactionType.引当)
                                         .GroupBy(x => x.ItemCode)
                                         .Select(x => new QueryByTerminalResult { ItemCode = x.Key, Quantity = x.Sum(xs => xs.Quantity) })
                                         .ToArrayAsync();

                var response = new QueryResponse<QueryByTerminalResult>
                {
                    Items = items
                };

                return new OkObjectResult(response);
            }
        }

        [FunctionName(nameof(QueryByStore))]
        public static async Task<IActionResult> QueryByStore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/query")]
            HttpRequest req,
            string companyCode,
            string storeCode,
            ILogger log)
        {
            using (var context = new StockDbContext())
            {
                var items = await context.Stocks
                                         .Where(x => x.CompanyCode == companyCode && x.StoreCode == storeCode && x.TransactionType != TransactionType.引当)
                                         .GroupBy(x => new { x.TerminalCode, x.ItemCode })
                                         .Select(x => new QueryByStoreResult { TerminalCode = x.Key.TerminalCode, ItemCode = x.Key.ItemCode, Quantity = x.Sum(xs => xs.Quantity) })
                                         .ToArrayAsync();

                var response = new QueryResponse<QueryByStoreResult>
                {
                    Items = items
                };

                return new OkObjectResult(response);
            }
        }
    }

    public class QueryByItem
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class QueryByTerminalResult
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class QueryByStoreResult
    {
        [JsonProperty("terminalCode")]
        public string TerminalCode { get; set; }

        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class QueryResponse<TResult>
    {
        [JsonProperty("items")]
        public TResult[] Items { get; set; }
    }
}
