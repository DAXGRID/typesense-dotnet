using System.Threading.Tasks;

namespace Typesense
{
    public interface ITypesenseClient
    {
        Task CreateCollection(Schema schema);
        Task CreateDocument(string schema, object document);
        Task<SearchResult<T>> Search<T>(string schema, SearchParameters searchParameters);
        Task<T> RetrieveDocument<T>(string collection, string id);
        Task<Collection> RetrieveCollection(string schema);
        Task<T> Delete<T>(string collection, string documentId);
        Task<FilterDeleteResponse> Delete(string collection, string filter, int batchSize);
    }
}
