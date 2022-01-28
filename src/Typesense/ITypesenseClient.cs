using System.Collections.Generic;
using System.Threading.Tasks;

namespace Typesense;

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
    Task<CollectionResponse> RetrieveCollection(string name);

    /// <summary>
    /// Retrieve all the collections.
    /// </summary>
    /// <returns>A list of collections.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    Task<List<CollectionResponse>> RetrieveCollections();

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
    Task<List<ImportResponse>> ImportDocuments<T>(string collection, List<T> documents, int batchSize = 40, ImportType importType = ImportType.Create);

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

    /// <summary>
    /// Creates an api key.
    /// </summary>
    /// <param name="key">Key to be inserted.</param>
    /// <returns>The created key.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<KeyResponse> CreateKey(Key key);

    /// <summary>
    /// Retrieve a key
    /// </summary>
    /// <param name="id">Id of key to be retrived</param>
    /// <returns>A single key.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    Task<KeyResponse> RetrieveKey(int id);

    /// <summary>
    /// Delete an api key.
    /// </summary>
    /// <param name="id">Id of key to be deleted.</param>
    /// <returns>A DeletedKeyResponse with an id of the deleted Key or default(DeleteKeyResponse) if it could not be found.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    Task<DeleteKeyResponse> DeleteKey(int id);

    /// <summary>
    /// List all api keys.
    /// </summary>
    /// <returns>List of all keys.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    Task<ListKeysResponse> ListKeys();

    /// <summary>
    /// Generate scoped search API keys without having to make any calls to the Typesense server.
    /// By using an API key with a search scope (only), create an HMAC digest of the parameters.
    /// With this scoped-generated key, you can use it as an API key.
    ///
    /// Remember to never expose your main/parent search key client-side.
    /// </summary>
    /// <param name="securityKey">Main/Parent API Key as the security</param>
    /// <param name="parameters">
    /// Embed search parameters as json. Example: "{"filter_by": "company_id:124", "expires_at": 1906054106}".
    /// You can also set a custom expires_at for a scoped API key. The expiration for a scoped API key should be less
    /// than the expiration of the parent API key with which it is generated. expires_at is optional.
    /// </param>
    /// <returns>Scope API key</returns>
    string GenerateScopedSearchKey(string securityKey, string parameters);

    /// <summary>
    /// Upsert search override.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="overrideName">The name of the search override.</param>
    /// <param name="searchOverride">The specificiation for the search override.</param>
    /// <returns>The upserted search override.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<SearchOverride> UpsertSearchOverride(string collection, string overrideName, SearchOverride searchOverride);

    /// <summary>
    /// Listing all search overrides associated with a given collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <returns>List of search overrides.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<ListSearchOverridesResponse> ListSearchOverrides(string collection);

    /// <summary>
    /// Fetch an individual override associated with a collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="overrideName">The override name that should be retrieved.</param>
    /// <returns>The search override or null if not found.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<SearchOverride> RetrieveSearchOverride(string collection, string overrideName);

    /// <summary>
    /// Deleting an override associated with a collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="overrideName">The override name that should be deleted.</param>
    /// <returns>The deleted search override.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<DeleteSearchOverrideResponse> DeleteSearchOverride(string collection, string overrideName);

    /// <summary>
    /// Upsert collection alias.
    /// </summary>
    /// <param name="aliasName">The alias name.</param>
    /// <param name="collectionAlias">The collection alias to be upserted.</param>
    /// <returns>The upserted collection alias.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<CollectionAlias> UpsertCollectionAlias(string aliasName, CollectionAlias collectionAlias);

    /// <summary>
    /// Retrieve alias on collection name.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <returns>The given alias on collection name. If not found return null.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<CollectionAlias> RetrieveCollectionAlias(string collection);

    /// <summary>
    /// List all aliases and the corresponding collections that they map to.
    /// </summary>
    /// <returns>List of aliases.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    Task<ListCollectionAliasesResponse> ListCollectionAliases();

    /// <summary>
    /// Delete alias on alias name.
    /// </summary>
    /// <param name="aliasName">The alias name.</param>
    /// <returns>The deleted collection alias.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<CollectionAlias> DeleteCollectionAlias(string aliasName);

    /// <summary>
    /// Upsert synonym.
    /// </summary>
    /// <param name="collection">Collection to insert the synonym into.</param>
    /// <param name="synonym">The name of the synonym.</param>
    /// <param name="schema">The synonym schema.</param>
    /// <returns>The created synonym.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<SynonymSchemaResponse> UpsertSynonym(string collection, string synonym, SynonymSchema schema);

    /// <summary>
    /// Retrieve synonym in collection on synonym name.
    /// </summary>
    /// <param name="collection">The synonym collection name.</param>
    /// <param name="synonym">The name of the synonym.</param>
    /// <returns>Synonym in colection on name or null if not found.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym);

    /// <summary>
    /// List all synonyms associated with a given collection.
    /// </summary>
    /// <param name="collection">Collection name.</param>
    /// <returns>All synonyms in collection or null if not found.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<ListSynonymsResponse> ListSynonyms(string collection);

    /// <summary>
    /// Delete a synonym associated with a collection.
    /// </summary>
    /// <param name="collection">Collection name.</param>
    /// <param name="synonym">Synonym name.</param>
    /// <returns>Id of the deleted synonym.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym);
}
