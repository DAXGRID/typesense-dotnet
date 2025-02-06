using System;
using System.Collections.Generic;
using System.Threading;
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
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionResponse> CreateCollection(Schema schema);

    /// <summary>
    /// Creates the document in the speicfied collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="document">The document to be inserted. The document should be in JSON format.</param>
    /// <returns>The created document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> CreateDocument<T>(string collection, string document) where T : class;

    /// <summary>
    /// Creates the document in the speicfied collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="document">The document to be inserted.</param>
    /// <returns>The created document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> CreateDocument<T>(string collection, T document) where T : class;

    /// <summary>
    /// Inserts the document if it does not exist or update if the document exist.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="document">The document to be inserted. The document should be in JSON format.</param>
    /// <returns>The created or updated document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> UpsertDocument<T>(string collection, string document) where T : class;

    /// <summary>
    /// Inserts the document if it does not exist or update if the document exist.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="document">The document to be upserted.</param>
    /// <returns>The created or updated document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> UpsertDocument<T>(string collection, T document) where T : class;

    /// <summary>
    /// Search for a document in the specified collection using the supplied search parameters.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="searchParameters">The search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search result.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters, CancellationToken ctk = default);

    /// <summary>
    /// Search for a document in the specified collection using the supplied search parameters, returning a grouped result.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="groupedSearchParameters">The search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    /// <returns>The search result.</returns>
    Task<SearchGroupedResult<T>> SearchGrouped<T>(string collection, GroupedSearchParameters groupedSearchParameters, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First collection of multi-search parameters.</param>
    /// <param name="limitMultiSearches">Max number of search requests that can be sent in a multi-search request. Eg: 20. Default is 50.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<MultiSearchResult<T1>>> MultiSearch<T1>(ICollection<MultiSearchParameters> s1, int? limitMultiSearches = null, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<MultiSearchResult<T1>> MultiSearch<T1>(MultiSearchParameters s1, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>)> MultiSearch<T1, T2>(MultiSearchParameters s1, MultiSearchParameters s2, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>)> MultiSearch<T1, T2, T3>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>)> MultiSearch<T1, T2, T3, T4>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="s5">Fifth multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>)> MultiSearch<T1, T2, T3, T4, T5>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, MultiSearchParameters s5, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="s5">Fifth multi-search parameters.</param>
    /// <param name="s6">Sixth multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>)> MultiSearch<T1, T2, T3, T4, T5, T6>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, MultiSearchParameters s5, MultiSearchParameters s6, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="s5">Fifth multi-search parameters.</param>
    /// <param name="s6">Sixth multi-search parameters.</param>
    /// <param name="s7">Seventh multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, MultiSearchParameters s5, MultiSearchParameters s6, MultiSearchParameters s7, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="s5">Fifth multi-search parameters.</param>
    /// <param name="s6">Sixth multi-search parameters.</param>
    /// <param name="s7">Seventh multi-search parameters.</param>
    /// <param name="s8">Eighth multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>, MultiSearchResult<T8>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7, T8>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, MultiSearchParameters s5, MultiSearchParameters s6, MultiSearchParameters s7, MultiSearchParameters s8, CancellationToken ctk = default);

    /// <summary>
    /// Multiple Searches for documents in the specified collections using the supplied search parameters.
    /// </summary>
    /// <param name="s1">First multi-search parameters.</param>
    /// <param name="s2">Second multi-search parameters.</param>
    /// <param name="s3">Third multi-search parameters.</param>
    /// <param name="s4">Fourth multi-search parameters.</param>
    /// <param name="s5">Fifth multi-search parameters.</param>
    /// <param name="s6">Sixth multi-search parameters.</param>
    /// <param name="s7">Seventh multi-search parameters.</param>
    /// <param name="s8">Eighth multi-search parameters.</param>
    /// <param name="s9">Ninth multi-search parameters.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search results.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>, MultiSearchResult<T8>, MultiSearchResult<T9>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        (MultiSearchParameters s1, MultiSearchParameters s2, MultiSearchParameters s3, MultiSearchParameters s4, MultiSearchParameters s5, MultiSearchParameters s6, MultiSearchParameters s7, MultiSearchParameters s8, MultiSearchParameters s9, CancellationToken ctk = default);

    /// <summary>
    /// Gets the document in the specified collection on id.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The document id.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The document or default(T) if the document could not be found.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> RetrieveDocument<T>(string collection, string id, CancellationToken ctk = default) where T : class;

    /// <summary>
    /// Updates the document in the specified collection on id.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The document id.</param>
    /// <param name="document">The document to be updated. The document should be in JSON format.</param>
    /// <returns>The updated document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> UpdateDocument<T>(string collection, string id, string document) where T : class;

    /// <summary>
    /// Updates the document in the specified collection on id.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The document id.</param>
    /// <param name="document">The document to be updated.</param>
    /// <returns>The updated document.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class;

    /// <summary>
    /// Retrieve the collection on collection name.
    /// </summary>
    /// <param name="name">The collection name.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The collection.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionResponse> RetrieveCollection(string name, CancellationToken ctk = default);

    /// <summary>
    /// Retrieve all the collections.
    /// </summary>
    /// <returns>A list of collections.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<CollectionResponse>> RetrieveCollections(CancellationToken ctk = default);

    /// <summary>
    /// Deletes a document in the collection on a specified document id.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="documentId">The id of the document to be deleted.</param>
    /// <returns>The deleted document or default(T) if it could not be found.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<T> DeleteDocument<T>(string collection, string documentId) where T : class;

    /// <summary>
    /// Deletes documents in a collection using the supplied filter.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="filter">The filter that is used to selected which documents that should be deleted.</param>
    /// <param name="batchSize">The number of documents that should deleted at a time.</param>
    /// <returns>A response containing a count of the deleted documents.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize = 40);

    /// <summary>
    /// Deletes documents in a collection using the supplied filter.
    /// </summary>
    /// <param name="name">The collection name.</param>
    /// <returns>A response with the collection deleted.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionResponse> DeleteCollection(string name);


    /// <summary>
    /// Update or alter a collection.
    /// </summary>
    /// <param name="name">The collection name.</param>
    /// <param name="updateSchema">The update schema definition.</param>
    /// <returns>The fields on the collection.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<UpdateCollectionResponse> UpdateCollection(string name, UpdateSchema updateSchema);

    /// <summary>
    /// Updates documents in a collection using the supplied filter.
    /// Partial update by default. Set full update to include everything in the update, also NULL values.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="document">A documents of type T to update the filtered collection list with.</param>
    /// <param name="filter">The filter that is used to selected which documents that should be updated.</param>
    /// <param name="filter">The filter that is used to selected which documents that should be updated.</param>
    /// <param name="fullUpdate">Includes everything in the document, including NULL values.</param>
    /// <returns>A response containing a count of the updated documents.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<FilterUpdateResponse> UpdateDocuments<T>(string collection, T document, string filter, bool fullUpdate = false);
    
    /// <summary>
    /// Batch import documents.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="documents">A list of the documents to be imported. The whole string should be in JSON-newline format.</param>
    /// <param name="batchSize">The number of documents that should be imported - defaults to 40.</param>
    /// <param name="importType">The import type, can either be Create, Update or Upsert - defaults to Create.</param>
    /// <param name="remoteEmbeddingBatchSize">
    /// Max size of each batch that will be sent to remote APIs while importing multiple documents at once.
    /// Using lower amount will lower timeout risk, but increase number of requests made.
    /// If not specified, typesense server will default to 200.
    /// </param>
    /// <param name="returnId">Eanble to return the id fo the document in the response.</param>
    /// <returns>A collection of import responses.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<ImportResponse>> ImportDocuments(
        string collection,
        string documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null);

    /// <summary>
    /// Batch import documents.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="documents">A list of the documents to be imported. Each document should be in JSON format.</param>
    /// <param name="batchSize">The number of documents that should be imported - defaults to 40.</param>
    /// <param name="importType">The import type, can either be Create, Update or Upsert - defaults to Create.</param>
    /// <param name="remoteEmbeddingBatchSize">
    /// Max size of each batch that will be sent to remote APIs while importing multiple documents at once.
    /// Using lower amount will lower timeout risk, but increase number of requests made.
    /// If not specified, typesense server will default to 200.
    /// </param>
    /// <param name="returnId">Eanble to return the id fo the document in the response.</param>
    /// <returns>A collection of import responses.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<ImportResponse>> ImportDocuments(
        string collection,
        IEnumerable<string> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null);

    /// <summary>
    /// Batch import documents.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="documents">A list of the documents to be imported.</param>
    /// <param name="batchSize">The number of documents that should be imported - defaults to 40.</param>
    /// <param name="importType">The import type, can either be Create, Update or Upsert - defaults to Create.</param>
    /// <param name="remoteEmbeddingBatchSize">
    /// Max size of each batch that will be sent to remote APIs while importing multiple documents at once.
    /// Using lower amount will lower timeout risk, but increase number of requests made.
    /// If not specified, typesense server will default to 200.
    /// </param>
    /// <param name="returnId">Eanble to return the id fo the document in the response.</param>
    /// <returns>A collection of import responses.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        IEnumerable<T> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null);

    /// <summary>
    /// Export all documents in a given collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<T>> ExportDocuments<T>(string collection, CancellationToken ctk = default);

    /// <summary>
    /// Export all documents in a given collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="exportParameters">Extra query parameters for exporting documents.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters, CancellationToken ctk = default);

    /// <summary>
    /// Creates an api key.
    /// </summary>
    /// <param name="key">Key to be inserted.</param>
    /// <returns>The created key.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<KeyResponse> CreateKey(Key key);

    /// <summary>
    /// Retrieve a key
    /// </summary>
    /// <param name="id">Id of key to be retrived</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>A single key.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<KeyResponse> RetrieveKey(int id, CancellationToken ctk = default);

    /// <summary>
    /// Delete an api key.
    /// </summary>
    /// <param name="id">Id of key to be deleted.</param>
    /// <returns>A DeletedKeyResponse with an id of the deleted Key or default(DeleteKeyResponse) if it could not be found.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<DeleteKeyResponse> DeleteKey(int id);

    /// <summary>
    /// List all api keys.
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>List of all keys.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<ListKeysResponse> ListKeys(CancellationToken ctk = default);

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
    /// <exception cref="ArgumentException"></exception>
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
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SearchOverrideResponse> UpsertSearchOverride(string collection, string overrideName, SearchOverride searchOverride);

    /// <summary>
    /// Listing all search overrides associated with a given collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>List of search overrides.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<ListSearchOverridesResponse> ListSearchOverrides(string collection, CancellationToken ctk = default);

    /// <summary>
    /// Fetch an individual override associated with a collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="overrideName">The override name that should be retrieved.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The search override.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SearchOverrideResponse> RetrieveSearchOverride(string collection, string overrideName, CancellationToken ctk = default);

    /// <summary>
    /// Deleting an override associated with a collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="overrideName">The override name that should be deleted.</param>
    /// <returns>The deleted search override.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
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
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionAliasResponse> UpsertCollectionAlias(string aliasName, CollectionAlias collectionAlias);

    /// <summary>
    /// Retrieve alias on collection name.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>The given alias on collection name.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionAliasResponse> RetrieveCollectionAlias(string collection, CancellationToken ctk = default);

    /// <summary>
    /// List all aliases and the corresponding collections that they map to.
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>List of aliases.</returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<ListCollectionAliasesResponse> ListCollectionAliases(CancellationToken ctk = default);

    /// <summary>
    /// Delete alias on alias name.
    /// </summary>
    /// <param name="aliasName">The alias name.</param>
    /// <returns>The deleted collection alias.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CollectionAliasResponse> DeleteCollectionAlias(string aliasName);

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
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiConflictException"></exception>
    /// <exception cref="TypesenseApiUnprocessableEntityException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SynonymSchemaResponse> UpsertSynonym(string collection, string synonym, SynonymSchema schema);

    /// <summary>
    /// Retrieve synonym in collection on synonym name.
    /// </summary>
    /// <param name="collection">The synonym collection name.</param>
    /// <param name="synonym">The name of the synonym.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>Synonym on name associated with the collection.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym, CancellationToken ctk = default);

    /// <summary>
    /// List all synonyms associated with a given collection.
    /// </summary>
    /// <param name="collection">Collection name.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>All synonyms associated with the collection.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<ListSynonymsResponse> ListSynonyms(string collection, CancellationToken ctk = default);

    /// <summary>
    /// Delete a synonym associated with a collection.
    /// </summary>
    /// <param name="collection">Collection name.</param>
    /// <param name="synonym">Synonym name.</param>
    /// <returns>Id of the deleted synonym.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym);

    /// <summary>
    /// Get current RAM, CPU, Disk and Network usage metrics.
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>
    /// Response containing current RAM, CPU, Disk and Network usage metrics.
    /// </returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<MetricsResponse> RetrieveMetrics(CancellationToken ctk = default);

    /// <summary>
    /// Get stats about API endpoints..
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>
    /// This endpoint returns average requests per second and latencies for all requests in the last 10 seconds.
    /// </returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<StatsResponse> RetrieveStats(CancellationToken ctk = default);


    /// <summary>
    /// Get health information about a Typesense node.
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>
    /// Returns the health information about a Typesense node.
    /// </returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<HealthResponse> RetrieveHealth(CancellationToken ctk = default);

    /// <summary>
    /// Asynchronously initiates a snapshot operation on the Typesense server.
    /// </summary>
    /// <param name="snapshotPath">The file system path on the Typesense server where the snapshot data will be written.</param>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the outcome of the snapshot creation.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<SnapshotResponse> CreateSnapshot(string snapshotPath, CancellationToken ctk = default);

    /// <summary>
    /// Asynchronously initiates the running of a compaction of the underlying RocksDB database.
    /// Note: While the database will not block during this operation, we recommend running it during off-peak hours.
    /// </summary>
    /// <param name="ctk">The optional cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the outcome of the successful compaction.
    /// </returns>
    /// <exception cref="TypesenseApiException"></exception>
    /// <exception cref="TypesenseApiBadRequestException"></exception>
    /// <exception cref="TypesenseApiNotFoundException"></exception>
    /// <exception cref="TypesenseApiServiceUnavailableException"></exception>
    Task<CompactDiskResponse> CompactDisk(CancellationToken ctk = default);
}
