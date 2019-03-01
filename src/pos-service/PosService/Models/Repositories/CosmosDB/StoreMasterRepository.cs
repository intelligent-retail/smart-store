using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// 店舗マスターリポジトリ
    /// </summary>
    public class StoreMasterRepository : MasterRepositoryBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public StoreMasterRepository(DocumentClient documentClient)
            : base(documentClient)
        {
        }

        /// <summary>マスター名称</summary>
        public override string MasterName => "StoreMaster";

        /// <summary>
        /// 店舗マスターを取得します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <returns>該当店舗が存在する場合は店舗マスター Document、存在しない場合は null</returns>
        public async Task<StoreMasterDocument> GetStoreAsync(string companyCode, string storeCode)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<StoreMasterDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c =>
                    c.Mastername == this.MasterName
                    && c.CompanyCode == companyCode
                    && c.StoreCode == storeCode)
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }

        //// sample data
        ////{
        ////  "mastername": "StoreMaster",
        ////  "companyCode": "00100",
        ////  "storeCode": "12345",
        ////  "storeName": "Smart Retail 六本木店"
        ////}
    }
}
