using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.CosmosDB
{
    public class SkuRepository
    {
        private const string DatabaseName = "smartretailboxmanagement";
        private const string SkuCollectionName = "Skus";

        private readonly DocumentClient _client;

        public SkuRepository(DocumentClient client)
        {
            this._client = client;
        }

        public async Task<(RepositoryResult result, Sku info)> FindBySkuCodeAsync(string companyCode, string storeCode, string skuCode)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, SkuCollectionName);
            var query = this._client.CreateDocumentQuery<Sku>(collectionUri)
                .Where(r => r.CompanyCode == companyCode && r.StoreCode == storeCode && r.SkuCode == skuCode)
                .AsDocumentQuery();
            var sku = (await query.ExecuteNextAsync<Sku>()).FirstOrDefault();

            return
                sku == null
                    ? (RepositoryResult.CreateNotFound($"SKU({skuCode}) is not found."), sku)
                    : (RepositoryResult.OK, sku);
        }
    }
}
