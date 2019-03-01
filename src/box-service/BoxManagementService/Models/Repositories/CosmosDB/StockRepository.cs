using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.CosmosDB
{
    public class StockRepository
    {
        private const string DatabaseName = "smartretailboxmanagement";
        private const string StockCollectionName = "Stocks";

        private readonly DocumentClient _client;

        public StockRepository(DocumentClient client)
        {
            this._client = client;
        }

        public async Task<(RepositoryResult result, IEnumerable<Stock> stocks)> GetStockDifferencesAsync(string boxId)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, StockCollectionName);
            var query = this._client.CreateDocumentQuery<Stock>(collectionUri)
                .Where(r => r.BoxId == boxId)
                .AsDocumentQuery();

            var stocks = (await query.ExecuteNextAsync<Stock>()).ToList();

            var diffQuery =
                from open in stocks.Where(r => r.EventType == "0")
                join close in stocks.Where(r => r.EventType == "1") on open.SkuCode equals close.SkuCode into gj
                from closex in gj.DefaultIfEmpty()
                let closeQuantity = (closex?.Quantity ?? 0)
                where open.Quantity != closeQuantity
                select new Stock
                {
                    BoxId = open.BoxId,
                    EventType = "-",
                    SkuCode = open.SkuCode,
                    Quantity = closeQuantity - open.Quantity
                };

            return (RepositoryResult.OK, diffQuery.ToList());
        }

        public async Task<RepositoryResult> SetStocksAsync(string boxId, string eventType, IList<(string skuCode, int quantity)> stocks)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, StockCollectionName);
            var query = this._client.CreateDocumentQuery<Stock>(collectionUri)
                .Where(r => r.BoxId == boxId && r.EventType == eventType)
                .AsDocumentQuery();

            // データが存在する場合は一旦削除する
            while (query.HasMoreResults)
            {
                foreach (var stock in await query.ExecuteNextAsync<Stock>())
                {
                    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, StockCollectionName, stock.Id);
                    await this._client.DeleteDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(stock.BoxId) });
                }
            }

            var updateAt = DateTime.Now;
            foreach (var (skuCode, quantity) in stocks)
            {
                var doc = new Stock()
                {
                    BoxId = boxId,
                    EventType = eventType,
                    SkuCode = skuCode,
                    Quantity = quantity,
                    UpdatedAt = updateAt
                };
                await this._client.CreateDocumentAsync(collectionUri, doc);
            }

            return RepositoryResult.OK;
        }
    }
}
