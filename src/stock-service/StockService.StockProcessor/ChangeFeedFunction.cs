using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using StockService.Documents;
using StockService.Entities;

namespace StockService.StockProcessor
{
    public class ChangeFeedFunction
    {
        public ChangeFeedFunction(StockDbContext context, TelemetryConfiguration telemetryConfiguration)
        {
            _context = context;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        private readonly StockDbContext _context;
        private readonly TelemetryClient _telemetryClient;

        [FunctionName(nameof(ChangeFeedFunction))]
        public async Task Run([CosmosDBTrigger("StockBackend", "StockTransaction", ConnectionStringSetting = "CosmosDBConnection", LeaseCollectionName = "leases", LeaseDatabaseName = "StockBackend",
                                  LeasesCollectionThroughput = 400, CreateLeaseCollectionIfNotExists = true, FeedPollDelay = 500)]
                              JArray input,
                              IBinder binder,
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
                Quantity = -xs.Quantity
            }));

            await _context.Stocks.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // SignalR Service への接続文字列がセットされている場合のみ有効化
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SignalRConnection", EnvironmentVariableTarget.Process)))
            {
                var signalRMessages = binder.Bind<IAsyncCollector<SignalRMessage>>(new SignalRAttribute { ConnectionStringSetting = "SignalRConnection", HubName = "monitor" });

                // 変更通知を SignalR で送信する
                foreach (var document in documents)
                {
                    foreach (var item in document.Items)
                    {
                        await signalRMessages.AddAsync(new SignalRMessage
                        {
                            Target = "update",
                            Arguments = new object[] { document.TerminalCode, item.ItemCode }
                        });
                    }
                }
            }

            // Application Insights に通知
            foreach (var document in documents)
            {
                _telemetryClient.TrackTrace("End Stock Processor", new Dictionary<string, string> { { "ActivityId", document.ActivityId } });
            }
        }
    }
}
 