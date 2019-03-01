using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// 支払マスターリポジトリ
    /// </summary>
    public class PaymentMasterRepository : MasterRepositoryBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public PaymentMasterRepository(DocumentClient documentClient)
            : base(documentClient)
        {
        }

        /// <summary>マスター名称</summary>
        public override string MasterName => "PaymentMaster";

        /// <summary>
        /// 支払マスターを取得する
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="paymentCode">支払コード</param>
        /// <returns>該当支払が存在する場合は支払マスター Document、存在しない場合は null</returns>
        public async Task<PaymentMasterDocument> GetPaymentAsync(string companyCode, string storeCode, string paymentCode)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<PaymentMasterDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c =>
                    c.Mastername == this.MasterName
                    && c.CompanyCode == companyCode
                    && c.StoreCode == storeCode
                    && c.PaymentCode == paymentCode)
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }

        //// sample data
        ////{
        ////  "mastername": "PaymentMaster",
        ////  "companyCode": "00100",
        ////  "storeCode": "12345",
        ////  "paymentCode": "01",
        ////  "description": "クレジット"
        ////}
    }
}
