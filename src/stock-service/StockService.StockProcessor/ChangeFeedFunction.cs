using System;
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
        public static async Task Run([CosmosDBTrigger("StockBackend", "StockTransaction", ConnectionStringSetting = "CosmosDBConnection", LeaseCollectionName = "leases", LeaseDatabaseName = "StockBackend",
                                         LeasesCollectionThroughput = 400, CreateLeaseCollectionIfNotExists = true, FeedPollDelay = 500)]
                                     JArray input,
                                     IBinder binder,
                                     ILogger log)
        {
            if (input == null || input.Count <= 0)
            {
                return;
            }

            // StockDocument �փf�V���A���C�Y
            var documents = input.ToObject<StockDocument[]>();

            // �݌ɏ��� SQL DB �ɏ�������
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

            using (var context = new StockDbContext())
            {
                await context.Stocks.AddRangeAsync(entities);
                await context.SaveChangesAsync();
            }

            // SignalR Service �ւ̐ڑ������񂪃Z�b�g����Ă���ꍇ�̂ݗL����
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SignalRConnection", EnvironmentVariableTarget.Process)))
            {
                var signalRMessages = binder.Bind<IAsyncCollector<SignalRMessage>>(new SignalRAttribute { ConnectionStringSetting = "SignalRConnection", HubName = "monitor" });

                // �ύX�ʒm�� SignalR �ő��M����
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

            // Application Insights �ɒʒm
            foreach (var document in documents)
            {
                _telemetryClient.TrackTrace("End Stock Processor", new Dictionary<string, string> { { "ActivityId", document.ActivityId } });
            }
        }

        private static readonly TelemetryClient _telemetryClient = new TelemetryClient();
    }
}
