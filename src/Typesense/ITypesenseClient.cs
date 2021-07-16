using System.Collections.Generic;
using System.Threading.Tasks;

namespace Typesense
{
    public interface ITypesenseClient
    {
        /// <summary>
        /// Creates the collection with the supplied schema
        /// </summary>
        /// <param name="schema">The schema for the collection be created.</param>
        /// <returns>The created collection.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<CollectionResponse> CreateCollection(Schema schema);

        /// <summary>
        /// Creates the document in the speicfied collection.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="document">The document to be inserted.</param>
        /// <returns>The created document.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<T> CreateDocument<T>(string collection, T document);

        /// <summary>
        /// Inserts the document if it does not exist or update if the document exist.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="document">The document to be upserted.</param>
        /// <returns>The created or updated document.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<T> UpsertDocument<T>(string collection, T document);

        /// <summary>
        /// Search for a document in the specified collection using the supplied search parameters.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="searchParameters">The search parameters.</param>
        /// <returns>The search result.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters);

        /// <summary>
        /// Gets the document in the specified collection on id.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="searchParameters">The search parameters.</param>
        /// <returns>The document or default(T) if the document could not be found.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<T> RetrieveDocument<T>(string collection, string id);

        /// <summary>
        /// Updates the document in the specified collection on id.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="id">The document id.</param>
        /// <param name="document">The document to be updated.</param>
        /// <returns>The updated document or null if the document could not be found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<T> UpdateDocument<T>(string collection, string id, T document);

        /// <summary>
        /// Retrieve the collection on collection name.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <returns>The collection or null if it could not be found.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<Collection> RetrieveCollection(string name);

        /// <summary>
        /// Retrieve all the collections.
        /// </summary>
        /// <returns>A list of collections.</returns>
        /// <exception cref="TypesenseApiException"></exception>
        Task<IReadOnlyCollection<Collection>> RetrieveCollections();

        /// <summary>
        /// Deletes a document in the collection on a specified document id.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="documentId">The id of the document to be deleted.</param>
        /// <returns>The deleted document or default(T) if it could not be found.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<T> DeleteDocument<T>(string collection, string documentId);

        /// <summary>
        /// Deletes documents in a collection using the supplied filter.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="filter">The filter that is used to selected which documents that should be deleted.</param>
        /// <param name="batchSize">The number of documents that should deleted at a time.</param>
        /// <returns>A response containing a count of the deleted documents.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize);

        /// <summary>
        /// Deletes documents in a collection using the supplied filter.
        /// </summary>
        /// <param name="name">The collection name.</param>
        /// <returns>A response with the collection deleted or null if it could not be found.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<CollectionResponse> DeleteCollection(string name);

        /// <summary>
        /// Batch import documents.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="documents">A list of the documents to be imported.</param>
        /// <param name="batchSize">The number of documents that should be imported - defaults to 40.</param>
        /// <param name="importType">The import type, can either be Create, Update or Upsert - defaults to Create.</param>
        /// <returns>A collection of import responses.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<IReadOnlyCollection<ImportResponse>> ImportDocuments<T>(string collection, List<T> documents, int batchSize = 40, ImportType importType = ImportType.Create);

        /// <summary>
        /// Export all documents in a given collection.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <returns>A collection of documents.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<List<T>> ExportDocuments<T>(string collection);

        /// <summary>
        /// Export all documents in a given collection.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="exportParameters">Extra query parameters for exporting documents.</param>
        /// <returns>A collection of documents.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TypesenseApiException"></exception>
        Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters);
    }
}
