using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// 端末マスターリポジトリ
    /// </summary>
    public class TerminalMasterRepository : MasterRepositoryBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public TerminalMasterRepository(DocumentClient documentClient)
            : base(documentClient)
        {
        }

        /// <summary>マスター名称</summary>
        public override string MasterName => "TerminalMaster";

        /// <summary>
        /// 端末マスターを取得します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <returns>該当端末が存在する場合は端末マスター Document、存在しない場合は null</returns>
        public async Task<TerminalMasterDocument> GetTerminalAsync(string companyCode, string storeCode, int terminalNo)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<TerminalMasterDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c =>
                    c.Mastername == this.MasterName
                    && c.CompanyCode == companyCode
                    && c.StoreCode == storeCode
                    && c.TerminalNo == terminalNo)
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }

        //// sample data
        ////{
        ////  "mastername": "TerminalMaster",
        ////  "companyCode": "00100",
        ////  "storeCode": "12345",
        ////  "terminalNo": 1,
        ////  "terminalName": "BOX 1",
        ////  "terminalType1": "",
        ////  "terminalType2": ""
        ////}
    }
}
