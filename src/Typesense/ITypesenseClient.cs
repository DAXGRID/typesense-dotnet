using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Typesense
{
    public interface ITypesenseClient
    {
        Task CreateCollection(Schema schema);
        Task CreateDocument(string schema, object document);
        Task Search(string schema, SearchParameters obj);

        Task ImportDocuments(string schema, List<object> items, string action );
        Task RetrieveCollections();
    }
}
