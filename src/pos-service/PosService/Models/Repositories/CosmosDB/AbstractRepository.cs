using System;
using System.Net;
using System.Threading.Tasks;
using PosService.Models.Documents;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace PosService.Models.Repositories.CosmosDB
{
    public abstract class AbstractRepository<TDocument>
        where TDocument : AbstractDocument
    {
        protected AbstractRepository(DocumentClient documentClient, string databaseId, string collectionId)
        {
            this.DatabaseId = databaseId;
            this.CollectionId = collectionId;
            this.Client = documentClient;
            this.DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
        }

        protected string DatabaseId { get; }

        protected string CollectionId { get; }

        protected DocumentClient Client { get; }

        protected Uri DocumentCollectionUri { get; }

        public async Task CreateAsync(TDocument document)
        {
            document.CreatedOn = DateTime.UtcNow;

            var response = await this.Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(this.DatabaseId, this.CollectionId), document);

            document.Id = response.Resource.Id;
        }

        public async Task<TDocument> GetAsync(string id, string partitionKey)
        {
            try
            {
                var opt = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };

                var response = await this.Client.ReadDocumentAsync<TDocument>(UriFactory.CreateDocumentUri(this.DatabaseId, this.CollectionId, id), opt);
                return response.Document;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default(TDocument);
            }
        }

        public async Task UpdateAsync(TDocument document)
        {
            try
            {
                document.UpdatedOn = DateTime.UtcNow;

                var condition = new AccessCondition { Condition = document.Etag, Type = AccessConditionType.IfMatch };
                var opt = new RequestOptions { AccessCondition = condition };

                await this.Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(this.DatabaseId, this.CollectionId, document.Id), document, opt);
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                // failed optimistic concurrency
                throw;
            }
        }

        public async Task DeleteAsync(TDocument document, string partitionKey)
        {
            try
            {
                var condition = new AccessCondition { Condition = document.Etag, Type = AccessConditionType.IfMatch };
                var opt = new RequestOptions { AccessCondition = condition, PartitionKey = new PartitionKey(partitionKey) };

                await this.Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(this.DatabaseId, this.CollectionId, document.Id), opt);
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                // failed optimistic concurrency
                throw;
            }
        }
    }
}
