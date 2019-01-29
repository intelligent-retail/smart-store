using System.Linq;
using System.Threading.Tasks;

using ItemService.ItemMaster.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace ItemService.ItemMaster
{
    public static class ItemFunction
    {
        [FunctionName("ItemFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/company/{companyCode}/store/{storeCode}/items")] HttpRequest req,
            string companyCode,
            string storeCode,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")]
            DocumentClient documentClient,
            ILogger log)
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
            {
                return new BadRequestObjectResult("Required POST data");
            }

            var request = JsonConvert.DeserializeObject<ItemRequest>(body);

            if (request.ItemCodes == null || request.ItemCodes.Length == 0)
            {
                return new BadRequestObjectResult("Required ItemCodes");
            }

            var collectionUri = UriFactory.CreateDocumentCollectionUri(companyCode, "Items");

            try
            {
                var items = await documentClient.CreateDocumentQuery<ItemDocument>(collectionUri, new FeedOptions { PartitionKey = new PartitionKey(storeCode) })
                                                .Where(x => request.ItemCodes.Contains(x.ItemCode))
                                                .AsDocumentQuery()
                                                .ExecuteNextAsync<ItemDocument>();

                var response = new ItemResponse
                {
                    Items = items.ToArray()
                };

                return new OkObjectResult(response);
            }
            catch (DocumentClientException)
            {
                return new NotFoundResult();
            }
        }
    }

    public class ItemRequest
    {
        [JsonProperty("itemCodes")]
        public string[] ItemCodes { get; set; }
    }

    public class ItemResponse
    {
        [JsonProperty("items")]
        public ItemDocument[] Items { get; set; }
    }
}
