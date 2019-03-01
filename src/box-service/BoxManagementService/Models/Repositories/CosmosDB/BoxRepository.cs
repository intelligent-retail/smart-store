using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.CosmosDB
{
    public class BoxRepository
    {
        private const string DatabaseName = "smartretailboxmanagement";
        private const string BoxManagementCollectionName = "BoxManagements";

        private readonly DocumentClient _client;

        public BoxRepository(DocumentClient client)
        {
            this._client = client;
        }

        /// <summary>
        /// 指定した BOX 識別文字列に関連付けられた BOX を取得します。
        /// </summary>
        /// <param name="boxId">BOX 識別文字列。</param>
        /// <returns>非同期の操作を表すタスク。TResult パラメーターの値には、処理結果および取得された BOX が含まれます。</returns>
        public async Task<(RepositoryResult result, BoxState info)> FindByBoxIdAsync(string boxId)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, BoxManagementCollectionName);

            var query = this._client.CreateDocumentQuery<BoxState>(collectionUri)
                .Where(r => r.BoxId == boxId)
                .AsDocumentQuery();

            var box = (await query.ExecuteNextAsync<BoxState>()).FirstOrDefault();

            return
                box == null
                    ? (RepositoryResult.CreateNotFound($"Box({boxId}) is not found."), box)
                    : (RepositoryResult.OK, box);
        }

        /// <summary>
        /// Boxとカートを結びつける
        /// </summary>
        /// <param name="boxId">Box識別文字列</param>
        /// <param name="deviceId">デバイス識別文字列（スマートフォンのデバイスID）</param>
        /// <param name="userId">ユーザー識別文字列</param>
        /// <param name="cartId">カート識別文字列</param>
        /// <returns>処理結果</returns>
        public async Task<RepositoryResult> SetCartIdAsync(string boxId, string deviceId, string userId, string cartId)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, BoxManagementCollectionName);

            var query = this._client.CreateDocumentQuery<BoxState>(collectionUri)
                .Where(r => r.BoxId == boxId)
                .AsDocumentQuery();
            var box = (await query.ExecuteNextAsync<BoxState>()).FirstOrDefault();
            if (box == null)
            {
                box = new BoxState();
            }

            box.BoxId = boxId;
            box.DeviceId = deviceId;
            box.UserId = userId;
            box.CartId = cartId;
            box.UpdatedAt = DateTime.Now;

            await this._client.UpsertDocumentAsync(collectionUri, box, new RequestOptions { PartitionKey = new PartitionKey(box.BoxId) });

            return RepositoryResult.OK;
        }
    }
}
