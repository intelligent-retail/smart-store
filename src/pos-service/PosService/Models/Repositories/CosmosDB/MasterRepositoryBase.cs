using PosService.Models.Documents;
using Microsoft.Azure.Documents.Client;

namespace PosService.Models.Repositories.CosmosDB
{
    /// <summary>
    /// マスターリポジトリベースクラス
    /// </summary>
    public abstract class MasterRepositoryBase : AbstractRepository<CartDocument>
    {
        /// <summary>Cosmos DB の ID</summary>
        private static readonly string MasterDatabaseId = "smartretailpos";

        /// <summary>Cosmos DB の コレクション ID</summary>
        private static readonly string MasterCollectionId = "PosMasters";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentClient">Cosmos DB クライアント</param>
        public MasterRepositoryBase(DocumentClient documentClient)
            : base(documentClient, MasterDatabaseId, MasterCollectionId)
        {
        }

        /// <summary>マスター名称</summary>
        public abstract string MasterName { get; }
    }
}
