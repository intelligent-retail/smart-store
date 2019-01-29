using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using StockService.Documents;
using StockService.Entities;

namespace StockService.StockProcessor
{
    public static class ChangeFeedFunction
    {
        [FunctionName(nameof(ChangeFeedFunction))]
        public static async Task Run([CosmosDBTrigger("StockBackend", "StockTransaction", ConnectionStringSetting = "CosmosDBConnection", LeaseCollectionName = "leases", LeaseDatabaseName = "StockLease", LeasesCollectionThroughput = 400, CreateLeaseCollectionIfNotExists = true)]
                                     JArray input,
                                     [SignalR(ConnectionStringSetting = "SignalRConnection", HubName = "monitor")]
                                     IAsyncCollector<SignalRMessage> signalRMessages,
                                     ILogger log)
        {
            if (input == null || input.Count <= 0)
            {
                return;
            }

            // StockDocument へデシリアライズ
            var documents = input.ToObject<StockDocument[]>();

            // 在庫情報を SQL DB に書き込む
            var entities = documents.SelectMany(x => x.Items.Select(xs => new StockEntity
                                    {
                                        DocumentId = x.Id,
                                        TransactionId = x.TransactionId,
                                        TransactionDate = x.TransactionDate,
                                        TransactionType = x.TransactionType,
                                        LocationCode = x.LocationCode,
                                        CompanyCode = x.CompanyCode,
                                        StoreCode = x.StoreCode,
                                        TerminalCode = x.TerminalCode,
                                        LineNo = xs.LineNo,
                                        ItemCode = xs.ItemCode,
                                        Quantity = xs.Quantity
                                    }));

            using (var context = new StockDbContext())
            {
                await context.Stocks.AddRangeAsync(entities);
                await context.SaveChangesAsync();
            }

            // 変更通知を SignalR で送信する
            foreach (var group in documents.GroupBy(x => new { x.CompanyCode, x.StoreCode }))
            {
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "update",
                    Arguments = new object[] { group.Key.CompanyCode, group.Key.StoreCode }
                });
            }

            // Application Insights に通知
            foreach (var document in documents)
            {
                _telemetryClient.TrackTrace("End Stock Processor", new Dictionary<string, string> { { "ActivityId", document.ActivityId } });
            }
        }

        private static readonly TelemetryClient _telemetryClient = new TelemetryClient();
    }
}
