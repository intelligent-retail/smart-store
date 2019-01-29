using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace StockService.StockQuery
{
    public static class QueryItemFunction
    {
        [FunctionName("QueryItemFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/query/item")] QueryItemRequest req,
            ILogger log)
        {
            using (var context = new StockDbContext())
            {
                var count = await context.Stocks
                                         .Where(x => x.CompanyCode == req.CompanyCode && x.StoreCode == req.StoreCode &&
                                                     x.TerminalCode == req.TerminalCode && x.ItemCode == req.ItemCode)
                                         .SumAsync(x => x.Quantity);

                var response = new QueryItemResponse
                {
                    ItemCode = req.ItemCode,
                    Count = count
                };

                return new OkObjectResult(response);
            }
        }
    }

    public class QueryItemRequest
    {
        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }

        [JsonProperty("storeCode")]
        public string StoreCode { get; set; }

        [JsonProperty("terminalCode")]
        public string TerminalCode { get; set; }

        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }
    }

    public class QueryItemResponse
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
