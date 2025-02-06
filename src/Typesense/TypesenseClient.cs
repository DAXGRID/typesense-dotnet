using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Typesense.HttpContents;
using Typesense.Setup;

namespace Typesense;

public class TypesenseClient : ITypesenseClient
{
    private static readonly MediaTypeHeaderValue JsonMediaTypeHeaderValue = MediaTypeHeaderValue.Parse($"{MediaTypeNames.Application.Json};charset={Encoding.UTF8.WebName}");
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerDefault = new();

    private readonly JsonSerializerOptions _jsonNameCaseInsensitiveTrue = new() { PropertyNameCaseInsensitive = true };

    private readonly JsonSerializerOptions _jsonOptionsCamelCaseIgnoreWritingNull = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TypesenseClient(IOptions<Config> config, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClient);

        var node = config.Value.Nodes.First();
        httpClient.BaseAddress = new Uri($"{node.Protocol}://{node.Host}:{node.Port}");
        httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", config.Value.ApiKey);
        _httpClient = httpClient;
        if (config.Value.JsonSerializerOptions is not null)
        {
            _jsonNameCaseInsensitiveTrue = new JsonSerializerOptions(config.Value.JsonSerializerOptions)
            {
                PropertyNameCaseInsensitive = true
            };

            _jsonOptionsCamelCaseIgnoreWritingNull = new JsonSerializerOptions(config.Value.JsonSerializerOptions)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    }

    public async Task<CollectionResponse> CreateCollection(Schema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        using var jsonContent = JsonContent.Create(schema, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Post<CollectionResponse>("/collections", jsonContent, jsonSerializerOptions: null).ConfigureAwait(false);
    }

    public Task<T> CreateDocument<T>(string collection, string document) where T : class
    {
        return PostDocument<T>(collection, document, upsert: false);
    }

    public Task<T> CreateDocument<T>(string collection, T document) where T : class
    {
        return PostDocument(collection, document, upsert: false);
    }

    public Task<T> UpsertDocument<T>(string collection, string document) where T : class
    {
        return PostDocument<T>(collection, document, upsert: true);
    }

    public Task<T> UpsertDocument<T>(string collection, T document) where T : class
    {
        return PostDocument(collection, document, upsert: true);
    }

    private async Task<T> PostDocument<T>(string collection, string document, bool upsert) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(document));

        using var stringContent = GetApplicationJsonStringContent(document);
        return await PostDocuments<T>(collection, stringContent, upsert).ConfigureAwait(false);
    }

    private async Task<T> PostDocument<T>(string collection, T document, bool upsert) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));

        using var jsonContent = JsonContent.Create(document, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await PostDocuments<T>(collection, jsonContent, upsert).ConfigureAwait(false);
    }

    private Task<T> PostDocuments<T>(string collection, HttpContent httpContent, bool upsert)
    {
        var path = upsert
            ? $"/collections/{collection}/documents?action=upsert"
            : $"/collections/{collection}/documents";
        return Post<T>(path, httpContent, _jsonNameCaseInsensitiveTrue);
    }

    private Task<TResult> SearchInternal<TResult>(string collection,
        SearchParameters searchParameters, CancellationToken ctk = default) where TResult : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));

        ArgumentNullException.ThrowIfNull(searchParameters);

        var parameters = CreateUrlParameters(searchParameters);
        return Get<TResult>($"/collections/{collection}/documents/search?{parameters}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters, CancellationToken ctk = default)
    {
        return SearchInternal<SearchResult<T>>(collection, searchParameters, ctk);
    }

    public Task<SearchGroupedResult<T>> SearchGrouped<T>(string collection, GroupedSearchParameters groupedSearchParameters, CancellationToken ctk = default)
    {
        return SearchInternal<SearchGroupedResult<T>>(collection, groupedSearchParameters, ctk);
    }

    public async Task<List<MultiSearchResult<T>>> MultiSearch<T>(ICollection<MultiSearchParameters> s1, int? limitMultiSearches = null, CancellationToken ctk = default)
    {
        var searches = new { Searches = s1 };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);

        var path = limitMultiSearches is null
            ? "/multi_search"
            : $"/multi_search?limit_multi_searches={limitMultiSearches}";

        var response = await Post<JsonElement>(path, json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? results.EnumerateArray().Select(searchResponse => HandleDeserializeMultiSearch<T>(searchResponse)).ToList()
            : throw new InvalidOperationException("Could not get 'results' property from multi-search response.");
    }

    public async Task<MultiSearchResult<T>> MultiSearch<T>(MultiSearchParameters s1, CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? HandleDeserializeMultiSearch<T>(results[0])
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>)> MultiSearch<T1, T2>(MultiSearchParameters s1, MultiSearchParameters s2, CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>)> MultiSearch<T1, T2, T3>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>)> MultiSearch<T1, T2, T3, T4>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return (response.TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>)> MultiSearch<T1, T2, T3, T4, T5>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        MultiSearchParameters s5,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4, s5 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]),
               HandleDeserializeMultiSearch<T5>(results[4]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>)> MultiSearch<T1, T2, T3, T4, T5, T6>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        MultiSearchParameters s5,
        MultiSearchParameters s6,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4, s5, s6 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]),
               HandleDeserializeMultiSearch<T5>(results[4]),
               HandleDeserializeMultiSearch<T6>(results[5]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        MultiSearchParameters s5,
        MultiSearchParameters s6,
        MultiSearchParameters s7,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4, s5, s6, s7 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]),
               HandleDeserializeMultiSearch<T5>(results[4]),
               HandleDeserializeMultiSearch<T6>(results[5]),
               HandleDeserializeMultiSearch<T7>(results[6]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>, MultiSearchResult<T8>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7, T8>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        MultiSearchParameters s5,
        MultiSearchParameters s6,
        MultiSearchParameters s7,
        MultiSearchParameters s8,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4, s5, s6, s7, s8 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]),
               HandleDeserializeMultiSearch<T5>(results[4]),
               HandleDeserializeMultiSearch<T6>(results[5]),
               HandleDeserializeMultiSearch<T7>(results[6]),
               HandleDeserializeMultiSearch<T8>(results[7]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>, MultiSearchResult<T3>, MultiSearchResult<T4>, MultiSearchResult<T5>, MultiSearchResult<T6>, MultiSearchResult<T7>, MultiSearchResult<T8>, MultiSearchResult<T9>)> MultiSearch<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        MultiSearchParameters s5,
        MultiSearchParameters s6,
        MultiSearchParameters s7,
        MultiSearchParameters s8,
        MultiSearchParameters s9,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 } };
        using var json = JsonContent.Create(searches, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]),
               HandleDeserializeMultiSearch<T5>(results[4]),
               HandleDeserializeMultiSearch<T6>(results[5]),
               HandleDeserializeMultiSearch<T7>(results[6]),
               HandleDeserializeMultiSearch<T8>(results[7]),
               HandleDeserializeMultiSearch<T9>(results[8]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public Task<T> RetrieveDocument<T>(string collection, string id, CancellationToken ctk = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(id));

        return Get<T>($"/collections/{collection}/documents/{id}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(document));

        using var stringContent = GetApplicationJsonStringContent(document);
        return await Patch<T>($"collections/{collection}/documents/{id}", stringContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));

        using var jsonContent = JsonContent.Create(document, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Patch<T>($"collections/{collection}/documents/{id}", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public Task<CollectionResponse> RetrieveCollection(string name, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        return Get<CollectionResponse>($"/collections/{name}", jsonSerializerOptions: null, ctk);
    }

    public Task<List<CollectionResponse>> RetrieveCollections(CancellationToken ctk = default)
    {
        return Get<List<CollectionResponse>>("/collections", jsonSerializerOptions: null, ctk);
    }

    public Task<T> DeleteDocument<T>(string collection, string documentId) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(documentId));

        return Delete<T>($"/collections/{collection}/documents/{documentId}", _jsonNameCaseInsensitiveTrue);
    }

    public Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize = 40)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(filter));
        if (batchSize < 0)
            throw new ArgumentException("has to be greater than 0", nameof(batchSize));

        return Delete<FilterDeleteResponse>($"/collections/{collection}/documents?filter_by={Uri.EscapeDataString(filter)}&batch_size={batchSize}", _jsonNameCaseInsensitiveTrue);
    }

    public Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        return Delete<CollectionResponse>($"/collections/{name}", _jsonNameCaseInsensitiveTrue);
    }

    public async Task<UpdateCollectionResponse> UpdateCollection(
        string name,
        UpdateSchema updateSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        using var jsonContent = JsonContent.Create(
            updateSchema,
            JsonMediaTypeHeaderValue,
             _jsonOptionsCamelCaseIgnoreWritingNull);

        return await Patch<UpdateCollectionResponse>($"/collections/{name}", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public async Task<FilterUpdateResponse> UpdateDocuments<T>(string collection, T document, string filter, bool fullUpdate = false)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (document == null)
            throw new ArgumentNullException(nameof(document), "cannot be null");
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(filter));

        using var jsonContent = JsonContent.Create(document, JsonMediaTypeHeaderValue, fullUpdate ? _jsonSerializerDefault : _jsonOptionsCamelCaseIgnoreWritingNull);

        return await Patch<FilterUpdateResponse>($"collections/{collection}/documents?filter_by={Uri.EscapeDataString(filter)}&action=update", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public async Task<List<ImportResponse>> ImportDocuments(
        string collection,
        string documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null)
    {
        if (string.IsNullOrWhiteSpace(documents))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(documents));
        using var stringContent = GetTextPlainStringContent(documents);
        return await ImportDocuments(collection, stringContent, batchSize, importType, remoteEmbeddingBatchSize, returnId).ConfigureAwait(false);
    }

    private async Task<List<ImportResponse>> ImportDocuments(
        string collection,
        HttpContent documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null)
    {
        ArgumentNullException.ThrowIfNull(documents);
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace", nameof(collection));

        var path = $"/collections/{collection}/documents/import?batch_size={batchSize}";

        if (remoteEmbeddingBatchSize.HasValue)
            path += $"&remote_embedding_batch_size={remoteEmbeddingBatchSize.Value}";

        path += importType switch
        {
            ImportType.Create => "&action=create",
            ImportType.Update => "&action=update",
            ImportType.Upsert => "&action=upsert",
            ImportType.Emplace => "&action=emplace",
            _ => throw new ArgumentException($"Could not handle {nameof(ImportType)} with name '{Enum.GetName(importType)}'", nameof(importType)),
        };

        if (returnId is not null)
        {
            path += $"&return_id={returnId}";
        }

        using var response = await _httpClient.PostAsync(path, documents).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, default).ConfigureAwait(false);

        List<ImportResponse> result = new();
        await foreach (var line in GetLines(response).ConfigureAwait(false))
        {
            var importResponse = JsonSerializer.Deserialize<ImportResponse>(line) ?? throw new ArgumentException("Null is not valid for documents.");
            result.Add(importResponse);
        }
        return result;
    }

    public async Task<List<ImportResponse>> ImportDocuments(
        string collection,
        IEnumerable<string> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null)
    {
        ArgumentNullException.ThrowIfNull(documents);

        using var streamLinesContent = new StreamStringLinesHttpContent(documents);
        return await ImportDocuments(collection, streamLinesContent, batchSize, importType, remoteEmbeddingBatchSize).ConfigureAwait(false);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        IEnumerable<T> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null,
        bool? returnId = null)
    {
        ArgumentNullException.ThrowIfNull(documents);

        using var streamJsonLinesContent = new StreamJsonLinesHttpContent<T>(documents, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await ImportDocuments(collection, streamJsonLinesContent, batchSize, importType, remoteEmbeddingBatchSize).ConfigureAwait(false);
    }

    public Task<List<T>> ExportDocuments<T>(string collection, CancellationToken ctk = default)
    {
        return ExportDocuments<T>(collection, new ExportParameters(), ctk);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        ArgumentNullException.ThrowIfNull(exportParameters);

        var parameters = CreateUrlParameters(exportParameters);
        var lines = GetLines($"/collections/{collection}/documents/export?{parameters}", ctk).ConfigureAwait(false);
        List<T> documents = new();
        await foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            documents.Add(JsonSerializer.Deserialize<T>(line, _jsonNameCaseInsensitiveTrue) ??
                          throw new ArgumentException("Null is not valid for documents"));
        }
        return documents;
    }

    public async Task<KeyResponse> CreateKey(Key key)
    {
        ArgumentNullException.ThrowIfNull(key);

        using var jsonContent = JsonContent.Create(key, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Post<KeyResponse>("/keys", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public Task<KeyResponse> RetrieveKey(int id, CancellationToken ctk = default)
    {
        return Get<KeyResponse>($"/keys/{id}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<DeleteKeyResponse> DeleteKey(int id)
    {
        return Delete<DeleteKeyResponse>($"/keys/{id}", _jsonNameCaseInsensitiveTrue);
    }

    public Task<ListKeysResponse> ListKeys(CancellationToken ctk = default)
    {
        return Get<ListKeysResponse>($"/keys", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public string GenerateScopedSearchKey(string securityKey, string parameters)
    {
        if (String.IsNullOrWhiteSpace(securityKey))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(securityKey));
        if (String.IsNullOrWhiteSpace(parameters))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(parameters));

        var securityKeyAsBuffer = Encoding.UTF8.GetBytes(securityKey);
        var parametersAsBuffer = Encoding.UTF8.GetBytes(parameters);

        using var hmac = new HMACSHA256(securityKeyAsBuffer);
        var hash = hmac.ComputeHash(parametersAsBuffer);
        var digest = Convert.ToBase64String(hash);
        var keyPrefix = securityKey[..4];
        var rawScopedKey = $"{digest}{keyPrefix}{parameters}";

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawScopedKey));
    }

    public async Task<SearchOverrideResponse> UpsertSearchOverride(
        string collection, string overrideName, SearchOverride searchOverride)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        ArgumentNullException.ThrowIfNull(searchOverride);

        using var jsonContent = JsonContent.Create(searchOverride, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Put<SearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public Task<ListSearchOverridesResponse> ListSearchOverrides(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));

        return Get<ListSearchOverridesResponse>($"collections/{collection}/overrides", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<SearchOverrideResponse> RetrieveSearchOverride(string collection, string overrideName, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        return Get<SearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<DeleteSearchOverrideResponse> DeleteSearchOverride(
        string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        return Delete<DeleteSearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", _jsonNameCaseInsensitiveTrue);
    }

    public async Task<CollectionAliasResponse> UpsertCollectionAlias(string aliasName, CollectionAlias collectionAlias)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(aliasName));

        ArgumentNullException.ThrowIfNull(collectionAlias);

        using var jsonContent = JsonContent.Create(collectionAlias, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Put<CollectionAliasResponse>($"/aliases/{aliasName}", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public Task<CollectionAliasResponse> RetrieveCollectionAlias(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        return Get<CollectionAliasResponse>($"/aliases/{collection}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<ListCollectionAliasesResponse> ListCollectionAliases(CancellationToken ctk = default)
    {
        return Get<ListCollectionAliasesResponse>("/aliases", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<CollectionAliasResponse> DeleteCollectionAlias(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null or whitespace.", nameof(aliasName));

        return Delete<CollectionAliasResponse>($"/aliases/{aliasName}", _jsonNameCaseInsensitiveTrue);
    }

    public async Task<SynonymSchemaResponse> UpsertSynonym(
        string collection, string synonym, SynonymSchema schema)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException("cannot be null or whitespace.", nameof(synonym));

        ArgumentNullException.ThrowIfNull(schema);

        using var jsonContent = JsonContent.Create(schema, JsonMediaTypeHeaderValue, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await Put<SynonymSchemaResponse>($"/collections/{collection}/synonyms/{synonym}", jsonContent, _jsonNameCaseInsensitiveTrue).ConfigureAwait(false);
    }

    public Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        return Get<SynonymSchemaResponse>($"/collections/{collection}/synonyms/{synonym}", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<ListSynonymsResponse> ListSynonyms(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        return Get<ListSynonymsResponse>($"/collections/{collection}/synonyms", _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        return Delete<DeleteSynonymResponse>($"/collections/{collection}/synonyms/{synonym}", _jsonNameCaseInsensitiveTrue);
    }

    public Task<MetricsResponse> RetrieveMetrics(CancellationToken ctk = default)
    {
        return Get<MetricsResponse>("/metrics.json", jsonSerializerOptions: null, ctk);
    }

    public Task<StatsResponse> RetrieveStats(CancellationToken ctk = default)
    {
        return Get<StatsResponse>("/stats.json", jsonSerializerOptions: null, ctk);
    }

    public Task<HealthResponse> RetrieveHealth(CancellationToken ctk = default)
    {
        return Get<HealthResponse>("/health", jsonSerializerOptions: null, ctk);
    }

    public Task<SnapshotResponse> CreateSnapshot(string snapshotPath, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(snapshotPath))
            throw new ArgumentException(
                "The snapshot path must not be null, empty or consist of whitespace characters only.",
                nameof(snapshotPath));

        return Post<SnapshotResponse>($"/operations/snapshot?snapshot_path={Uri.EscapeDataString(snapshotPath)}", httpContent: null, _jsonNameCaseInsensitiveTrue, ctk);
    }

    public Task<CompactDiskResponse> CompactDisk(CancellationToken ctk = default)
    {
        return Post<CompactDiskResponse>("/operations/db/compact", httpContent: null, _jsonNameCaseInsensitiveTrue, ctk);
    }

    private static string CreateUrlParameters<T>(T queryParameters)
        where T : notnull
    {
        // Add all non-null properties to the query
        var parameters = queryParameters.GetType()
            .GetProperties()
            .Select(prop =>
            {
                var value = prop.GetValue(queryParameters);

                var stringValue = value switch
                {
                    null => null,
                    true => "true",
                    false => "false",
                    Enum e => e.ToString().ToLowerInvariant(),
                    _ => value.ToString(),
                };

                return new
                {
                    Key = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name,
                    Value = stringValue,
                };
            })
            .Where(parameter => parameter.Value != null && parameter.Key != null);

        return string.Join("", parameters.Select(_ => $"&{Uri.EscapeDataString(_.Key!)}={Uri.EscapeDataString(_.Value!)}"));
    }

    private async Task<T> Get<T>(string path, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private async IAsyncEnumerable<string> GetLines(string path, [EnumeratorCancellation] CancellationToken ctk = default)
    {
        using var response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        await foreach (var p in GetLines(response, ctk).ConfigureAwait(false))
            yield return p;
    }

    private static async IAsyncEnumerable<string> GetLines(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken ctk = default)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ctk).ConfigureAwait(false);
        using StreamReader streamReader = new(stream);

        while (true)
        {
            var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
                break;
            yield return line;
        }
    }

    private async Task<T> Delete<T>(string path, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var response = await _httpClient.DeleteAsync(path, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private async Task<T> Post<T>(string path, HttpContent? httpContent, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var response = await _httpClient.PostAsync(path, httpContent, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private async Task<T> Patch<T>(string path, HttpContent? httpContent, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var response = await _httpClient.PatchAsync(path, httpContent, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private async Task<T> Put<T>(string path, HttpContent? httpContent, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var response = await _httpClient.PutAsync(path, httpContent, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private static async Task<T> ReadJsonFromResponseMessage<T>(JsonSerializerOptions? jsonSerializerOptions, HttpResponseMessage response, CancellationToken ctk)
    {
        return await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, ctk).ConfigureAwait(false) ?? throw new ArgumentException("Deserialize is not allowed to return null.");
    }

    private static async Task GetException(HttpResponseMessage response, CancellationToken ctk)
    {
        var message = await response.Content.ReadAsStringAsync(ctk).ConfigureAwait(false);
        throw GetException(response.StatusCode, message);
    }

    private static TypesenseApiException GetException(HttpStatusCode statusCode, string message)
        => statusCode switch
        {
            HttpStatusCode.BadRequest => new TypesenseApiBadRequestException(message),
            HttpStatusCode.Unauthorized => new TypesenseApiUnauthorizedException(message),
            HttpStatusCode.NotFound => new TypesenseApiNotFoundException(message),
            HttpStatusCode.Conflict => new TypesenseApiConflictException(message),
            HttpStatusCode.UnprocessableEntity => new TypesenseApiUnprocessableEntityException(message),
            HttpStatusCode.ServiceUnavailable => new TypesenseApiUnprocessableEntityException(message),
            // If we receive a status code that has not been documented, we throw TypesenseApiException.
            _ => throw new TypesenseApiException($"Received an unspecified status-code: '{Enum.GetName(statusCode)}' from Typesense, with message: '{message}'.")
        };

    private static StringContent GetApplicationJsonStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, MediaTypeNames.Application.Json);

    private static StringContent GetTextPlainStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "text/plain");

    private MultiSearchResult<T> HandleDeserializeMultiSearch<T>(JsonElement jsonElement)
        => jsonElement.Deserialize<MultiSearchResult<T>>(_jsonNameCaseInsensitiveTrue)
        ?? throw new InvalidOperationException($"Could not deserialize {typeof(T)}, Received following from Typesense: '{jsonElement}'.");
}
