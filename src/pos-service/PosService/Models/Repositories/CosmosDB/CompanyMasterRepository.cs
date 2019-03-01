using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// 企業マスターリポジトリ
    /// </summary>
    public class CompanyMasterRepository : MasterRepositoryBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public CompanyMasterRepository(DocumentClient documentClient)
            : base(documentClient)
        {
        }

        /// <summary>マスター名称</summary>
        public override string MasterName => "CompanyMaster";

        /// <summary>
        /// 企業マスターを取得します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <returns>該当企業が存在する場合は企業マスター Document、存在しない場合は null</returns>
        public async Task<CompanyMasterDocument> GetCompanyAsync(string companyCode)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<CompanyMasterDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c =>
                    c.Mastername == this.MasterName
                    && c.CompanyCode == companyCode)
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }

        //// sample data
        ////{
        ////  "mastername": "CompanyMaster",
        ////  "companyCode": "00100",
        ////  "companyName": "Smart Retailer Ltd."
        ////}
    }
}
