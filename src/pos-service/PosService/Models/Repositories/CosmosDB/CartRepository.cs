using System;
using System.Linq;
using System.Threading.Tasks;
using PosService.Constants;
using PosService.Models.Documents;
using PosService.Utilities;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// カートリポジトリ
    /// </summary>
    public class CartRepository : AbstractRepository<CartDocument>
    {
        /// <summary>Cosmos DB の ID</summary>
        private static readonly string CartsDatabaseId = "smartretailpos";

        /// <summary>Cosmos DB の コレクション ID</summary>
        private static readonly string CartsCollectionId = "Carts";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public CartRepository(DocumentClient documentClient)
            : base(documentClient, CartsDatabaseId, CartsCollectionId)
        {
        }

        /// <summary>
        /// カートを作成します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="storeName">店舗名</param>
        /// <param name="terminalNo">端末番号</param>
        /// <param name="userId">ユーザー ID</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="receiptNo">レシート番号</param>
        /// <param name="transactionNo">取引番号</param>
        /// <returns>作成したカート Document</returns>
        public async Task<CartDocument> CreateCartAsync(
            string companyCode,
            string storeCode,
            string storeName,
            int terminalNo,
            string userId,
            string userName,
            long receiptNo,
            long transactionNo)
        {
            // HACK：同一カートの同時利用を抑制する排他制御が必要な場合は実装する
            var cart = new CartDocument()
            {
                CartId = Guid.NewGuid().ToString(),
                Status = CartStatus.EnteringItem.Code,
                GenerateDateTime = DateTimeUtility.GetApplicationTimeNow(),
                CompanyCode = companyCode,
                StoreCode = storeCode,
                StoreName = storeName,
                TerminalNo = terminalNo,
                TransactionNo = transactionNo,
                ReceiptNo = receiptNo,
                ReceiptText = string.Empty,
                User = new UserInfo { UserId = userId, UserName = userName },
            };

            cart.Sales.ReferenceDateTime = cart.GenerateDateTime;
            await this.CreateAsync(cart);
            return cart;
        }

        /// <summary>
        /// カートを取得します
        /// </summary>
        /// <param name="cartId">カート ID</param>
        /// <returns>該当カートが存在する場合はカート Document、存在しない場合は null</returns>
        public async Task<CartDocument> GetCartAsync(string cartId)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<CartDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c => c.CartId == cartId)
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }
    }
}
