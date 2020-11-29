using System.Threading.Tasks;

namespace Typesense
{
    public interface ITypesenseClient
    {
        Task CreateCollection(Schema schema);
        Task CreateDocument(string schema, object document);
        Task UpsertDocument(string collection, object document);
        Task<SearchResult<T>> Search<T>(string schema, SearchParameters searchParameters);
        Task<T> RetrieveDocument<T>(string collection, string id);
        Task<T> UpdateDocument<T>(string collection, string id, T document);
        Task<Collection> RetrieveCollection(string schema);
        Task<T> DeleteDocument<T>(string collection, string documentId);
        Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize);
        Task<DeleteCollectionResponse> DeleteCollection(string name);
    }
}
