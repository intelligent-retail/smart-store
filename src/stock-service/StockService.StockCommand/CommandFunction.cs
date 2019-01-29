using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using StockService.Documents;

namespace StockService.StockCommand
{
    public static class CommandFunction
    {
        private const string DatabaseId = "StockBackend";
        private const string CollectionId = "StockTransaction";

        [FunctionName(nameof(CommandFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/stocks")]
            HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")]
            DocumentClient documentClient,
            ILogger log)
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
            {
                return new BadRequestObjectResult("Required POST data");
            }

            var document = JsonConvert.DeserializeObject<StockDocument>(body);

            // TransactionType を検証
            if (document.TransactionType != TransactionType.仮引当 && document.TransactionType != TransactionType.引当)
            {
                return new BadRequestObjectResult($"Invalid TransactionType: {document.TransactionType}");
            }

            // 追跡用の ActivityId をセット
            document.ActivityId = Activity.Current?.Id;

            _telemetryClient.TrackTrace("Begin Stock Command", new Dictionary<string, string> { { "ActivityId", document.ActivityId } });

            // Database / Collection を作成
            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId }, new RequestOptions { OfferThroughput = 400 });

            var documentCollection = new DocumentCollection
            {
                Id = CollectionId
            };

            documentCollection.PartitionKey.Paths.Add("/terminalCode");

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseId), documentCollection);

            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);

            // 本引当の時だけ TransactionId の重複チェックを入れる
            if (document.TransactionType == TransactionType.引当)
            {
                // 要求された TransactionId に対して Cosmos DB へクエリを投げる
                var transactions = await documentClient.CreateDocumentQuery<StockDocument>(collectionUri, new FeedOptions { PartitionKey = new PartitionKey(document.TerminalCode) })
                                                       .Where(x => x.TransactionId == document.TransactionId)
                                                       .AsDocumentQuery()
                                                       .ExecuteNextAsync<StockDocument>();

                // 仮引当の Transaction があるのかを確認
                if (transactions.Any(x => x.TransactionType != TransactionType.仮引当))
                {
                    return new BadRequestObjectResult($"Invalid TransactionId: {document.TransactionId}");
                }

                // 取得した Transaction に含まれている Items をマージ
                var stockItems = transactions.SelectMany(x => x.Items)
                                             .GroupBy(x => x.ItemCode)
                                             .OrderBy(x => x.Key)
                                             .Select(x => new StockItemDocument { ItemCode = x.Key, Quantity = x.Sum(xs => xs.Quantity) });

                // 仮引当と引当のデータの整合性を確認
                if (!stockItems.SequenceEqual(document.Items.OrderBy(x => x.ItemCode), new StockItemEqualityComparer()))
                {
                    return new BadRequestObjectResult($"Consistency Error TransactionId: {document.TransactionId}");
                }
            }

            // Cosmos DB に Transaction を作成する
            await documentClient.CreateDocumentAsync(collectionUri, document);

            return new OkObjectResult("OK");
        }

        private static readonly TelemetryClient _telemetryClient = new TelemetryClient();

        internal class StockItemEqualityComparer : IEqualityComparer<StockItemDocument>
        {
            public bool Equals(StockItemDocument x, StockItemDocument y)
            {
                return x.ItemCode == y.ItemCode && x.Quantity == y.Quantity;
            }

            public int GetHashCode(StockItemDocument obj)
            {
                return obj.ItemCode.GetHashCode() ^ obj.Quantity.GetHashCode();
            }
        }
    }
}
