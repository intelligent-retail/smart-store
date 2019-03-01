using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.CosmosDB
{
    public class TerminalRepository
    {
        private const string DatabaseName = "smartretailboxmanagement";
        private const string TerminalCollectionName = "Terminals";

        private readonly DocumentClient _client;

        public TerminalRepository(DocumentClient client)
        {
            this._client = client;
        }

        public async Task<(RepositoryResult result, Terminal info)> FindByBoxIdAsync(string boxId)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, TerminalCollectionName);
            var query = this._client.CreateDocumentQuery<Terminal>(collectionUri)
                .Where(r => r.BoxId == boxId)
                .AsDocumentQuery();
            var terminal = (await query.ExecuteNextAsync<Terminal>()).FirstOrDefault();

            return
                terminal == null
                    ? (RepositoryResult.CreateNotFound($"Box({boxId}) is not found."), terminal)
                    : (RepositoryResult.OK , terminal);
        }
    }
}
