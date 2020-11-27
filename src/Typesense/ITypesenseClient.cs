using System.Threading.Tasks;

namespace Typesense
{
    public interface ITypesenseClient
    {
        Task CreateCollection(Schema schema);
        Task CreateDocument(string schema, object document);
        Task<SearchResult> Search(string schema, SearchParameters obj);
        Task RetrieveCollections();
        Task<Collection> RetrieveCollection(string schema);
    }
}
