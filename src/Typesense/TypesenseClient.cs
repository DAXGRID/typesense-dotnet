using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        if (config is null)
            throw new ArgumentNullException(nameof(config));
        if (httpClient is null)
            throw new ArgumentNullException(nameof(httpClient));

        var node = config.Value.Nodes.First();
        httpClient.BaseAddress = new Uri($"{node.Protocol}://{node.Host}:{node.Port}");
        httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", config.Value.ApiKey);
        _httpClient = httpClient;
    }

    public async Task<CollectionResponse> CreateCollection(Schema schema)
    {
        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        var json = JsonSerializer.Serialize(schema, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post($"/collections", json).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response);
    }

    public async Task<T> CreateDocument<T>(string collection, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(document));

        var response = await Post($"/collections/{collection}/documents", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> CreateDocument<T>(string collection, T document) where T : class
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await CreateDocument<T>(collection, json).ConfigureAwait(false);
    }

    public async Task<T> UpsertDocument<T>(string collection, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(document));

        var response = await Post($"/collections/{collection}/documents?action=upsert", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpsertDocument<T>(string collection, T document) where T : class
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await UpsertDocument<T>(collection, json);
    }

    private async Task<TResult> SearchInternal<TResult>(string collection,
        SearchParameters searchParameters, CancellationToken ctk = default) where TResult : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (searchParameters is null)
            throw new ArgumentNullException(nameof(searchParameters));

        var parameters = CreateUrlParameters(searchParameters);
        var response = await Get($"/collections/{collection}/documents/search?{parameters}", ctk).ConfigureAwait(false);

        return HandleEmptyStringJsonSerialize<TResult>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters, CancellationToken ctk = default)
    {
        return await SearchInternal<SearchResult<T>>(collection, searchParameters, ctk);
    }

    public async Task<SearchGroupedResult<T>> SearchGrouped<T>(string collection, GroupedSearchParameters groupedSearchParameters, CancellationToken ctk = default)
    {
        return await SearchInternal<SearchGroupedResult<T>>(collection, groupedSearchParameters, ctk);
    }

    public async Task<SearchResult<T>> MultiSearch<T>(MultiSearchParameters s1, CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1 } };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post("/multi_search", json, ctk).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? HandleDeserializeMultiSearch<T>(results[0])
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(SearchResult<T1>, SearchResult<T2>)> MultiSearch<T1, T2>(MultiSearchParameters s1, MultiSearchParameters s2, CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2 } };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post("/multi_search", json, ctk).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(SearchResult<T1>, SearchResult<T2>, SearchResult<T3>)> MultiSearch<T1, T2, T3>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3 } };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post("/multi_search", json, ctk).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(SearchResult<T1>, SearchResult<T2>, SearchResult<T3>, SearchResult<T4>)> MultiSearch<T1, T2, T3, T4>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3,
        MultiSearchParameters s4,
        CancellationToken ctk = default)
    {
        var searches = new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4 } };
        var json = JsonSerializer.Serialize(searches, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post("/multi_search", json, ctk).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<T> RetrieveDocument<T>(string collection, string id, CancellationToken ctk = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(id));

        var response = await Get($"/collections/{collection}/documents/{id}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, string document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(document))
            throw new ArgumentException("Cannot be null empty or whitespace", nameof(document));

        var response = await Patch($"collections/{collection}/documents/{id}", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var json = JsonSerializer.Serialize(document, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await UpdateDocument<T>(collection, id, json).ConfigureAwait(false);
    }

    public async Task<CollectionResponse> RetrieveCollection(string name, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var response = await Get($"/collections/{name}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response);
    }

    public async Task<List<CollectionResponse>> RetrieveCollections(CancellationToken ctk = default)
    {
        var response = await Get($"/collections", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<List<CollectionResponse>>(response);
    }

    public async Task<T> DeleteDocument<T>(string collection, string documentId) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(documentId));

        var response = await Delete($"/collections/{collection}/documents/{documentId}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize = 40)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(filter));
        if (batchSize < 0)
            throw new ArgumentException("has to be greater than 0", nameof(batchSize));

        var response = await Delete($"/collections/{collection}/documents?filter_by={Uri.EscapeDataString(filter)}&batch_size={batchSize}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<FilterDeleteResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var response = await Delete($"/collections/{name}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<UpdateCollectionResponse> UpdateCollection(
        string name,
        UpdateSchema updateSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var json = JsonSerializer.Serialize(
            updateSchema,
            _jsonOptionsCamelCaseIgnoreWritingNull);

        var response = await Patch($"/collections/{name}", json).ConfigureAwait(false);

        return HandleEmptyStringJsonSerialize<UpdateCollectionResponse>(
            response,
            _jsonNameCaseInsentiveTrue);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        string documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(documents))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(documents));

        var path = $"/collections/{collection}/documents/import?batch_size={batchSize}";
        switch (importType)
        {
            case ImportType.Create:
                path += "&action=create";
                break;
            case ImportType.Update:
                path += "&action=update";
                break;
            case ImportType.Upsert:
                path += "&action=upsert";
                break;
            case ImportType.Emplace:
                path += "&action=emplace";
                break;
            default:
                throw new ArgumentException($"Could not handle {nameof(ImportType)} with name '{Enum.GetName(importType)}'", nameof(importType));
        }

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
        ImportType importType = ImportType.Create)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        var jsonNewlines = JsonNewLines(documents);
        return await ImportDocuments<T>(collection, jsonNewlines, batchSize, importType).ConfigureAwait(false);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection,
        IEnumerable<T> documents,
        int batchSize = 40,
        ImportType importType = ImportType.Create)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        var jsonNewlines = JsonNewLines(documents, _jsonOptionsCamelCaseIgnoreWritingNull);
        return await ImportDocuments<T>(collection, jsonNewlines, batchSize, importType).ConfigureAwait(false);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, CancellationToken ctk = default)
    {
        return await ExportDocuments<T>(collection, new ExportParameters(), ctk).ConfigureAwait(false);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));
        if (exportParameters is null)
            throw new ArgumentNullException(nameof(exportParameters));

        var parameters = CreateUrlParameters(exportParameters);
        var response = await Get($"/collections/{collection}/documents/export?{parameters}", ctk).ConfigureAwait(false);

        return response.Split('\n')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select((x) => JsonSerializer.Deserialize<T>(x, _jsonNameCaseInsentiveTrue)
                    ?? throw new ArgumentException("Null is not valid for documents"))
            .ToList();
    }

    public async Task<KeyResponse> CreateKey(Key key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var json = JsonSerializer.Serialize(key, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Post($"/keys", json).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<KeyResponse> RetrieveKey(int id, CancellationToken ctk = default)
    {
        var response = await Get($"/keys/{id}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteKeyResponse> DeleteKey(int id)
    {
        var response = await Delete($"/keys/{id}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<DeleteKeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListKeysResponse> ListKeys(CancellationToken ctk = default)
    {
        var response = await Get($"/keys", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListKeysResponse>(response, _jsonNameCaseInsentiveTrue);
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
        if (searchOverride is null)
            throw new ArgumentNullException(nameof(searchOverride));

        var json = JsonSerializer.Serialize(searchOverride, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Put($"/collections/{collection}/overrides/{overrideName}", json).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSearchOverridesResponse> ListSearchOverrides(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));

        var response = await Get($"collections/{collection}/overrides", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListSearchOverridesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchOverrideResponse> RetrieveSearchOverride(string collection, string overrideName, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        var response = await Get($"/collections/{collection}/overrides/{overrideName}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteSearchOverrideResponse> DeleteSearchOverride(
        string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        var response = await Delete($"/collections/{collection}/overrides/{overrideName}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<DeleteSearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAliasResponse> UpsertCollectionAlias(string aliasName, CollectionAlias collectionAlias)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(aliasName));
        if (collectionAlias is null)
            throw new ArgumentNullException(nameof(collectionAlias));

        var json = JsonSerializer.Serialize(collectionAlias, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Put($"/aliases/{aliasName}", json).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAliasResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAliasResponse> RetrieveCollectionAlias(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        var response = await Get($"/aliases/{collection}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAliasResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListCollectionAliasesResponse> ListCollectionAliases(CancellationToken ctk = default)
    {
        var response = await Get("/aliases", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListCollectionAliasesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAliasResponse> DeleteCollectionAlias(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("cannot be null or whitespace.", nameof(aliasName));

        var response = await Delete($"/aliases/{aliasName}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAliasResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> UpsertSynonym(
        string collection, string synonym, SynonymSchema schema)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException("cannot be null or whitespace.", nameof(synonym));
        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        var json = JsonSerializer.Serialize(schema, _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await Put($"/collections/{collection}/synonyms/{synonym}", json).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms/{synonym}", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSynonymsResponse> ListSynonyms(string collection, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListSynonymsResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        var response = await Delete($"/collections/{collection}/synonyms/{synonym}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<DeleteSynonymResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<MetricsResponse> RetrieveMetrics(CancellationToken ctk = default)
    {
        var response = await Get("/metrics.json", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<MetricsResponse>(response);
    }

    public async Task<StatsResponse> RetrieveStats(CancellationToken ctk = default)
    {
        var response = await Get("/stats.json", ctk).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<StatsResponse>(response);
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

    private async Task<string> Get(string path, CancellationToken ctk = default)
    {
        var (response, responseString) = await HandleHttpResponse(_httpClient.GetAsync, path, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Delete(string path, CancellationToken ctk = default)
    {
        var (response, responseString) = await HandleHttpResponse(_httpClient.DeleteAsync, path, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Post(string path, string json, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PostAsync, path, stringContent, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Patch(string path, string json, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PatchAsync, path, stringContent, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Put(string path, string json, CancellationToken ctk = default)
    {
        using var stringContent = GetApplicationJsonStringContent(json);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PutAsync, path, stringContent, ctk).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
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

    private static async Task<(HttpResponseMessage response, string responseString)> HandleHttpResponse(
        Func<string, HttpContent, CancellationToken, Task<HttpResponseMessage>> f, string path, StringContent stringContent, CancellationToken ctk)
    {
        var response = await f(path, stringContent, ctk).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(ctk).ConfigureAwait(false);
        return (response, responseString);
    }

    private static StringContent GetApplicationJsonStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "application/json");

    private static StringContent GetTextPlainStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "text/plain");

    private static T HandleEmptyStringJsonSerialize<T>(string json) where T : class
        => HandleEmptyStringJsonSerialize<T>(json, null);

    private static T HandleEmptyStringJsonSerialize<T>(string json, JsonSerializerOptions? options) where T : class
        => !string.IsNullOrEmpty(json)
        ? JsonSerializer.Deserialize<T>(json, options) ?? throw new ArgumentException("Deserialize is not allowed to return null.")
        : throw new ArgumentException("Empty JSON response is not valid.");

    private static string JsonNewLines(IEnumerable<string> documents)
        => String.Join('\n', documents);

    private static string JsonNewLines<T>(IEnumerable<T> documents, JsonSerializerOptions jsonOptions)
        => JsonNewLines(documents.Select(x => JsonSerializer.Serialize(x, jsonOptions)));

    private SearchResult<T> HandleDeserializeMultiSearch<T>(JsonElement jsonElement)
        => jsonElement.Deserialize<SearchResult<T>>(_jsonNameCaseInsentiveTrue)
        ?? throw new InvalidOperationException($"Could not deserialize {typeof(T)}, Received following from Typesense: '{jsonElement}'.");
}
