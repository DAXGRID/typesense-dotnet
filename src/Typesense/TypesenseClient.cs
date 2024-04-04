using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Typesense.Setup;

namespace Typesense;

public class TypesenseClient : ITypesenseClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonNameCaseInsentiveTrue = new() { PropertyNameCaseInsensitive = true };
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
    }

    public Task<CollectionResponse> CreateCollection(Schema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        var json = JsonSerializer.Serialize(schema, _jsonOptionsCamelCaseIgnoreWritingNull);
        return Post<CollectionResponse>("/collections", json, jsonSerializerOptions: null);
    }

    public Task<T> CreateDocument<T>(string collection, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(document));

        return Post<T>($"/collections/{collection}/documents", document, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> CreateDocument<T>(string collection, T document) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await CreateDocument<T>(collection, json).ConfigureAwait(false);
    }

    public Task<T> UpsertDocument<T>(string collection, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(document));

        return Post<T>($"/collections/{collection}/documents?action=upsert", document, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpsertDocument<T>(string collection, T document) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await UpsertDocument<T>(collection, json);
    }

    private Task<TResult> SearchInternal<TResult>(string collection,
        SearchParameters searchParameters, CancellationToken ctk = default) where TResult : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));

        ArgumentNullException.ThrowIfNull(searchParameters);

        var parameters = CreateUrlParameters(searchParameters);
        return Get<TResult>($"/collections/{collection}/documents/search?{parameters}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters, CancellationToken ctk = default)
    {
        return await SearchInternal<SearchResult<T>>(collection, searchParameters, ctk);
    }

    public async Task<SearchGroupedResult<T>> SearchGrouped<T>(string collection, GroupedSearchParameters groupedSearchParameters, CancellationToken ctk = default)
    {
        return await SearchInternal<SearchGroupedResult<T>>(collection, groupedSearchParameters, ctk);
    }

    public async Task<List<MultiSearchResult<T>>> MultiSearch<T>(ICollection<MultiSearchParameters> s1, int? limitMultiSearches = null, CancellationToken ctk = default)
    {
        var searches = new { Searches = s1 };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);

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
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return response.TryGetProperty("results", out var results)
            ? HandleDeserializeMultiSearch<T>(results[0])
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(MultiSearchResult<T1>, MultiSearchResult<T2>)> MultiSearch<T1, T2>(MultiSearchParameters s1, MultiSearchParameters s2, CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2 } };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
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
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
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
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post<JsonElement>("/multi_search", json, jsonSerializerOptions: null, ctk).ConfigureAwait(false);

        return (response.TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public Task<T> RetrieveDocument<T>(string collection, string id, CancellationToken ctk = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(id));

        return Get<T>($"/collections/{collection}/documents/{id}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<T> UpdateDocument<T>(string collection, string id, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(document));

        return Patch<T>($"collections/{collection}/documents/{id}", document, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await UpdateDocument<T>(collection, id, json).ConfigureAwait(false);
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

        return Delete<T>($"/collections/{collection}/documents/{documentId}", _jsonNameCaseInsentiveTrue);
    }

    public Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize = 40)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(filter));
        if (batchSize < 0)
            throw new ArgumentException("has to be greater than 0", nameof(batchSize));

        return Delete<FilterDeleteResponse>($"/collections/{collection}/documents?filter_by={Uri.EscapeDataString(filter)}&batch_size={batchSize}", _jsonNameCaseInsentiveTrue);
    }

    public Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        return Delete<CollectionResponse>($"/collections/{name}", _jsonNameCaseInsentiveTrue);
    }

    public Task<UpdateCollectionResponse> UpdateCollection(
        string name,
        UpdateSchema updateSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var json = JsonSerializer.Serialize(
            updateSchema,
            _jsonOptionsCamelCaseIgnoreWritingNull);

        return Patch<UpdateCollectionResponse>($"/collections/{name}", json, _jsonNameCaseInsentiveTrue);
    }

    public Task<FilterUpdateResponse> UpdateDocuments<T>(string collection, T document, string filter)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (document == null)
            throw new ArgumentNullException(nameof(document), "cannot be null");
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(filter));
    
        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
    
        return Patch<FilterUpdateResponse>($"collections/{collection}/documents?filter_by={Uri.EscapeDataString(filter)}&action=update", json, _jsonNameCaseInsentiveTrue);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        string documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(documents))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(documents));

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

        using var stringContent = GetTextPlainStringContent(documents);
        var response = await _httpClient.PostAsync(path, stringContent).ConfigureAwait(false);
        var responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));

        return response.IsSuccessStatusCode
            ? responseString.Split('\n').Select(
                (x) => JsonSerializer.Deserialize<ImportResponse>(x) ?? throw new ArgumentException("Null is not valid for documents.")).ToList()
            : throw new TypesenseApiException(responseString);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        IEnumerable<string> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var jsonNewlines = JsonNewLines(documents);
        return await ImportDocuments<T>(collection, jsonNewlines, batchSize, importType, remoteEmbeddingBatchSize).ConfigureAwait(false);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        IEnumerable<T> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create,
        int? remoteEmbeddingBatchSize = null)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var jsonNewlines = JsonNewLines(documents, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await ImportDocuments<T>(collection, jsonNewlines, batchSize, importType, remoteEmbeddingBatchSize).ConfigureAwait(false);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, CancellationToken ctk = default)
    {
        return await ExportDocuments<T>(collection, new ExportParameters(), ctk).ConfigureAwait(false);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        ArgumentNullException.ThrowIfNull(exportParameters);

        var parameters = CreateUrlParameters(exportParameters);
        var response = await Get($"/collections/{collection}/documents/export?{parameters}", ctk).ConfigureAwait(false);

        return response.Split('\n')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select((x) => JsonSerializer.Deserialize<T>(x, _jsonNameCaseInsentiveTrue)
                    ?? throw new ArgumentException("Null is not valid for documents"))
            .ToList();
    }

    public Task<KeyResponse> CreateKey(Key key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var json = JsonSerializer.Serialize(key, _jsonOptionsCamelCaseIgnoreWritingNull);
        return Post<KeyResponse>($"/keys", json, _jsonNameCaseInsentiveTrue);
    }

    public Task<KeyResponse> RetrieveKey(int id, CancellationToken ctk = default)
    {
        return Get<KeyResponse>($"/keys/{id}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<DeleteKeyResponse> DeleteKey(int id)
    {
        return Delete<DeleteKeyResponse>($"/keys/{id}", _jsonNameCaseInsentiveTrue);
    }

    public Task<ListKeysResponse> ListKeys(CancellationToken ctk = default)
    {
        return Get<ListKeysResponse>($"/keys", _jsonNameCaseInsentiveTrue, ctk);
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

    public Task<SearchOverrideResponse> UpsertSearchOverride(
        string collection, string overrideName, SearchOverride searchOverride)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        ArgumentNullException.ThrowIfNull(searchOverride);

        var json = JsonSerializer.Serialize(searchOverride, _jsonOptionsCamelCaseIgnoreWritingNull);
        return Put<SearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", json, _jsonNameCaseInsentiveTrue);
    }

    public Task<ListSearchOverridesResponse> ListSearchOverrides(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));

        return Get<ListSearchOverridesResponse>($"collections/{collection}/overrides", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<SearchOverrideResponse> RetrieveSearchOverride(string collection, string overrideName, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        return Get<SearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<DeleteSearchOverrideResponse> DeleteSearchOverride(
        string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        return Delete<DeleteSearchOverrideResponse>($"/collections/{collection}/overrides/{overrideName}", _jsonNameCaseInsentiveTrue);
    }

    public Task<CollectionAliasResponse> UpsertCollectionAlias(string aliasName, CollectionAlias collectionAlias)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(aliasName));

        ArgumentNullException.ThrowIfNull(collectionAlias);

        var json = JsonSerializer.Serialize(collectionAlias, _jsonOptionsCamelCaseIgnoreWritingNull);
        return Put<CollectionAliasResponse>($"/aliases/{aliasName}", json, _jsonNameCaseInsentiveTrue);
    }

    public Task<CollectionAliasResponse> RetrieveCollectionAlias(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        return Get<CollectionAliasResponse>($"/aliases/{collection}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<ListCollectionAliasesResponse> ListCollectionAliases(CancellationToken ctk = default)
    {
        return Get<ListCollectionAliasesResponse>("/aliases", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<CollectionAliasResponse> DeleteCollectionAlias(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null or whitespace.", nameof(aliasName));

        return Delete<CollectionAliasResponse>($"/aliases/{aliasName}", _jsonNameCaseInsentiveTrue);
    }

    public Task<SynonymSchemaResponse> UpsertSynonym(
        string collection, string synonym, SynonymSchema schema)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException("cannot be null or whitespace.", nameof(synonym));

        ArgumentNullException.ThrowIfNull(schema);

        var json = JsonSerializer.Serialize(schema, _jsonOptionsCamelCaseIgnoreWritingNull);
        return Put<SynonymSchemaResponse>($"/collections/{collection}/synonyms/{synonym}", json, _jsonNameCaseInsentiveTrue);
    }

    public Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        return Get<SynonymSchemaResponse>($"/collections/{collection}/synonyms/{synonym}", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<ListSynonymsResponse> ListSynonyms(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        return Get<ListSynonymsResponse>($"/collections/{collection}/synonyms", _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        return Delete<DeleteSynonymResponse>($"/collections/{collection}/synonyms/{synonym}", _jsonNameCaseInsentiveTrue);
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

        return Post<SnapshotResponse>($"/operations/snapshot?snapshot_path={Uri.EscapeDataString(snapshotPath)}", httpContent: null, _jsonNameCaseInsentiveTrue, ctk);
    }

    public Task<CompactDiskResponse> CompactDisk(CancellationToken ctk = default)
    {
        return Post<CompactDiskResponse>("/operations/db/compact", httpContent: null, _jsonNameCaseInsentiveTrue, ctk);
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

    private async Task<string> Get(string path, CancellationToken ctk = default)
    {
        var (response, responseString) = await HandleHttpResponse(_httpClient.GetAsync, path, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
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

    private async Task<T> Post<T>(string path, string json, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        return await Post<T>(path, stringContent, jsonSerializerOptions, ctk).ConfigureAwait(false);
    }

    private async Task<T> Patch<T>(string path, string json, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        using var response = await _httpClient.PatchAsync(path, stringContent, ctk).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await GetException(response, ctk).ConfigureAwait(false);

        return await ReadJsonFromResponseMessage<T>(jsonSerializerOptions, response, ctk).ConfigureAwait(false);
    }

    private async Task<T> Put<T>(string path, string json, JsonSerializerOptions? jsonSerializerOptions, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        using var response = await _httpClient.PutAsync(path, stringContent, ctk).ConfigureAwait(false);
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
            _ => throw new ArgumentException($"Could not convert statuscode {Enum.GetName(statusCode)}.")
        };

    private static async Task<(HttpResponseMessage response, string responseString)> HandleHttpResponse(
        Func<string, CancellationToken, Task<HttpResponseMessage>> f, string path, CancellationToken ctk)
    {
        var response = await f(path, ctk).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(ctk).ConfigureAwait(false);
        return (response, responseString);
    }

    private static StringContent GetApplicationJsonStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "application/json");

    private static StringContent GetTextPlainStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "text/plain");

    private static string JsonNewLines(IEnumerable<string> documents)
        => String.Join('\n', documents);

    private static string JsonNewLines<T>(IEnumerable<T> documents, JsonSerializerOptions jsonOptions)
        => JsonNewLines(documents.Select(x => JsonSerializer.Serialize(x, jsonOptions)));

    private MultiSearchResult<T> HandleDeserializeMultiSearch<T>(JsonElement jsonElement)
        => jsonElement.Deserialize<MultiSearchResult<T>>(_jsonNameCaseInsentiveTrue)
        ?? throw new InvalidOperationException($"Could not deserialize {typeof(T)}, Received following from Typesense: '{jsonElement}'.");
}
