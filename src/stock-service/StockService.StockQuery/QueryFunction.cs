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
                                                     x.TerminalCode == terminalCode)
                                         .GroupBy(x => x.ItemCode)
                                         .Select(x => new QueryByTerminalItem { ItemCode = x.Key, Quantity = x.Sum(xs => xs.Quantity) })
                                         .ToArrayAsync();

                var response = new QueryResponse<QueryByTerminalItem>
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
                                         .Where(x => x.CompanyCode == companyCode && x.StoreCode == storeCode)
                                         .GroupBy(x => new { x.TerminalCode, x.ItemCode })
                                         .Select(x => new QueryByStoreItem { TerminalCode = x.Key.TerminalCode, ItemCode = x.Key.ItemCode, Quantity = x.Sum(xs => xs.Quantity) })
                                         .ToArrayAsync();

                var response = new QueryResponse<QueryByStoreItem>
                {
                    Items = items
                };

                return new OkObjectResult(response);
            }
        }
    }

    public class QueryResponse<T>
    {
        [JsonProperty("items")]
        public T[] Items { get; set; }
    }

    public class QueryByTerminalItem
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class QueryByStoreItem
    {
        [JsonProperty("terminalCode")]
        public string TerminalCode { get; set; }

        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
