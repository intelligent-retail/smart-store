using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using PosService.Utilities;
using Microsoft.Azure.Documents.Client;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// 取引ログリポジトリ
    /// </summary>
    public class TransactionLogRepository : AbstractRepository<TransactionLogDocument>
    {
        /// <summary>Cosmos DB の ID</summary>
        private static readonly string TranLogsDatabaseId = "smartretailpos";

        /// <summary>Cosmos DB の コレクション ID</summary>
        private static readonly string TranLogsCollectionId = "TransactionLogs";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public TransactionLogRepository(DocumentClient documentClient)
            : base(documentClient, TranLogsDatabaseId, TranLogsCollectionId)
        {
        }

        /// <summary>
        /// カート情報から取引ログを作成します
        /// </summary>
        /// <param name="cart">カート Document</param>
        /// <returns>作成した取引ログ Document</returns>
        public async Task<TransactionLogDocument> CreateTransactionLogAsync(CartDocument cart)
        {
            var tranDoc = new TransactionLogDocument
            {
                CompanyCode = cart.CompanyCode,
                StoreCode = cart.StoreCode,
                StoreName = cart.StoreName,
                TerminalNo = cart.TerminalNo,
                TransactionNo = cart.TransactionNo,
                TransactionType = cart.TransactionType,
                BusinessDate = DateTimeUtility.GetApplicationTimeNow().Date,
                GenerateDateTime = DateTimeUtility.GetApplicationTimeNow(),
                ReceiptNo = cart.ReceiptNo,
                User = cart.User,
                Sales = cart.Sales,
                LineItems = cart.LineItems.Select(l => (TransactionLogBase.LineItem)l).ToList(),
                Payments = cart.Payments,
                Taxes = cart.Taxes
            };

            await this.CreateAsync(tranDoc);
            return tranDoc;
        }
    }
}
