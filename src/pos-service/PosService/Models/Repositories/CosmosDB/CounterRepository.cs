using System.Linq;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// ビジネスカウンターリポジトリ
    /// </summary>
    public class CounterRepository : AbstractRepository<CounterDocument>
    {
        /// <summary>Cosmos DB の ID</summary>
        private static readonly string CounterDatabaseId = "smartretailpos";

        /// <summary>Cosmos DB の コレクション ID</summary>
        private static readonly string CounterCollectionId = "Counters";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public CounterRepository(DocumentClient documentClient)
            : base(documentClient, CounterDatabaseId, CounterCollectionId)
        {
        }

        /// <summary>
        /// カウンター値を採番します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <param name="countType">カウンター種別</param>
        /// <returns>採番後のカウンター値</returns>
        public async Task<long> NumberingCount(string companyCode, string storeCode, int terminalNo, string countType)
            => await this.InsertUpdateCounterAsync(companyCode, storeCode, terminalNo, countType);

        /// <summary>
        /// カウンター値を新規作成または更新します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <param name="countType">カウンター種別</param>
        /// <returns>採番後のカウンター値</returns>
        private async Task<long> InsertUpdateCounterAsync(string companyCode, string storeCode, int terminalNo, string countType)
        {
            var doc = await this.GetCounterAsync(companyCode, storeCode, terminalNo, countType);

            if (doc == null)
            {
                doc = new CounterDocument()
                {
                    TerminalKey = this.CreateDocumentId(companyCode, storeCode, terminalNo)
                };

                doc.CountDic.Add(countType, 1L);
                await this.CreateAsync(doc);
            }
            else
            {
                if (doc.CountDic.ContainsKey(countType))
                {
                    doc.CountDic[countType]++;
                }
                else
                {
                    doc.CountDic.Add(countType, 1L);
                }

                await this.UpdateAsync(doc);
            }

            return doc.CountDic[countType];
        }

        /// <summary>
        /// カウンター値を取得します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <param name="countType">カウンター種別</param>
        /// <returns>カウンター値</returns>
        private async Task<CounterDocument> GetCounterAsync(string companyCode, string storeCode, int terminalNo, string countType)
        {
            var qDoc = this.Client
                .CreateDocumentQuery<CounterDocument>(this.DocumentCollectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(c => c.TerminalKey == this.CreateDocumentId(companyCode, storeCode, terminalNo))
                .AsDocumentQuery();

            return (await qDoc.ExecuteNextAsync()).FirstOrDefault();
        }

        /// <summary>
        /// Document の ID を作成します
        /// </summary>
        /// <param name="companyCode">企業コード</param>
        /// <param name="storeCode">店舗コード</param>
        /// <param name="terminalNo">端末番号</param>
        /// <returns>Document の ID</returns>
        private string CreateDocumentId(string companyCode, string storeCode, int terminalNo)
            => $"{companyCode}_{storeCode}_{terminalNo}";
    }
}
