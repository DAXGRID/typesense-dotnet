using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        var response = await Post($"/collections", schema).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response);
    }

    public async Task<T> CreateDocument<T>(string collection, T document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var response = await Post($"/collections/{collection}/documents", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpsertDocument<T>(string collection, T document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var response = await Post($"/collections/{collection}/documents?action=upsert", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    private async Task<TResult> SearchInternal<TResult>(string collection,
        SearchParameters searchParameters) where TResult : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (searchParameters is null)
            throw new ArgumentNullException(nameof(searchParameters));

        var parameters = CreateUrlSearchParameters(searchParameters);
        var response = await Get($"/collections/{collection}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}").ConfigureAwait(false);

        return HandleEmptyStringJsonSerialize<TResult>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters)
    {
        return await SearchInternal<SearchResult<T>>(collection, searchParameters);
    }

    public async Task<SearchGroupedResult<T>> SearchGrouped<T>(string collection, GroupedSearchParameters groupedSearchParameters)
    {
        return await SearchInternal<SearchGroupedResult<T>>(collection, groupedSearchParameters);
    }

    public async Task<SearchResult<T>> MultiSearch<T>(MultiSearchParameters s1)
    {
        var response = await Post("/multi_search", new { Searches = new MultiSearchParameters[] { s1 } }).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? HandleDeserializeMultiSearch<T>(results[0])
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(SearchResult<T1>, SearchResult<T2>)> MultiSearch<T1, T2>(MultiSearchParameters s1, MultiSearchParameters s2)
    {
        var response = await Post("/multi_search", new { Searches = new MultiSearchParameters[] { s1, s2 } }).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<(SearchResult<T1>, SearchResult<T2>, SearchResult<T3>)> MultiSearch<T1, T2, T3>(
        MultiSearchParameters s1,
        MultiSearchParameters s2,
        MultiSearchParameters s3)
    {
        var response = await Post("/multi_search", new { Searches = new MultiSearchParameters[] { s1, s2, s3 } }).ConfigureAwait(false);

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
        MultiSearchParameters s4)
    {
        var response = await Post("/multi_search", new { Searches = new MultiSearchParameters[] { s1, s2, s3, s4 } }).ConfigureAwait(false);

        return (JsonSerializer.Deserialize<JsonElement>(response).TryGetProperty("results", out var results))
            ? (HandleDeserializeMultiSearch<T1>(results[0]),
               HandleDeserializeMultiSearch<T2>(results[1]),
               HandleDeserializeMultiSearch<T3>(results[2]),
               HandleDeserializeMultiSearch<T4>(results[3]))
            : throw new InvalidOperationException("Could not get results from multi-search result.");
    }

    public async Task<T> RetrieveDocument<T>(string collection, string id) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(id));

        var response = await Get($"/collections/{collection}/documents/{id}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(collection));
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var response = await Patch($"collections/{collection}/documents/{id}", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> RetrieveCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var response = await Get($"/collections/{name}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response);
    }

    public async Task<List<CollectionResponse>> RetrieveCollections()
    {
        var response = await Get($"/collections").ConfigureAwait(false);
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


        var response = await Delete($"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<FilterDeleteResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("cannot be null empty or whitespace", nameof(name));

        var response = await Delete($"/collections/{name}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection, IEnumerable<T> documents, int batchSize = 40, ImportType importType = ImportType.Create)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace", nameof(collection));
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

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

        var jsonNewlines = CreateJsonNewlines(documents, _jsonOptionsCamelCaseIgnoreWritingNull);
        using var stringContent = GetTextPlainStringContent(jsonNewlines);
        var response = await _httpClient.PostAsync(path, stringContent).ConfigureAwait(false);
        var responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));

        return response.IsSuccessStatusCode
            ? responseString.Split('\n').Select((x) => JsonSerializer.Deserialize<ImportResponse>(x)
                                                ?? throw new ArgumentException("Null is not valid for documents.")).ToList()
            : throw new TypesenseApiException(responseString);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection)
    {
        return await ExportDocuments<T>(collection, new ExportParameters()).ConfigureAwait(false);
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));
        if (exportParameters is null)
            throw new ArgumentNullException(nameof(exportParameters));

        var extraParameters = new List<string>();
        if (exportParameters.IncludeFields is not null)
            extraParameters.Add($"include_fields={exportParameters.IncludeFields}");
        if (exportParameters.FilterBy is not null)
            extraParameters.Add($"filter_by={exportParameters.FilterBy}");
        if (exportParameters.ExcludeFields is not null)
            extraParameters.Add($"exclude_fields={exportParameters.ExcludeFields}");

        var searchParameters = string.Join("&", extraParameters);
        var response = await Get($"/collections/{collection}/documents/export?{searchParameters}").ConfigureAwait(false);

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

        var response = await Post($"/keys", key).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<KeyResponse> RetrieveKey(int id)
    {
        var response = await Get($"/keys/{id}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteKeyResponse> DeleteKey(int id)
    {
        var response = await Delete($"/keys/{id}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<DeleteKeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListKeysResponse> ListKeys()
    {
        var response = await Get($"/keys").ConfigureAwait(false);
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

        var response = await Put($"/collections/{collection}/overrides/{overrideName}", searchOverride).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSearchOverridesResponse> ListSearchOverrides(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));

        var response = await Get($"collections/{collection}/overrides").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListSearchOverridesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchOverrideResponse> RetrieveSearchOverride(string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(collection));
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException("cannot be null, empty or whitespace.", nameof(overrideName));

        var response = await Get($"/collections/{collection}/overrides/{overrideName}").ConfigureAwait(false);
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

        var response = await Put($"/aliases/{aliasName}", collectionAlias).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAliasResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAliasResponse> RetrieveCollectionAlias(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("cannot be null or whitespace.", nameof(collection));

        var response = await Get($"/aliases/{collection}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAliasResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListCollectionAliasesResponse> ListCollectionAliases()
    {
        var response = await Get("/aliases").ConfigureAwait(false);
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

        var response = await Put($"/collections/{collection}/synonyms/{synonym}", schema).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms/{synonym}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSynonymsResponse> ListSynonyms(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms").ConfigureAwait(false);
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

    private static string CreateUrlSearchParameters(SearchParameters searchParameters)
    {
        var urlParameters = "";

        if (searchParameters.MaxHits is not null)
            urlParameters += $"&max_hits={searchParameters.MaxHits}";
        if (searchParameters.QueryByWeights is not null)
            urlParameters += $"&query_by_weights={searchParameters.QueryByWeights}";
        if (searchParameters.Prefix is not null)
            urlParameters += $"&prefix={searchParameters.Prefix.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.FilterBy is not null)
            urlParameters += $"&filter_by={searchParameters.FilterBy}";
        if (searchParameters.SortBy is not null)
            urlParameters += $"&sort_by={searchParameters.SortBy}";
        if (searchParameters.FacetBy is not null)
            urlParameters += $"&facet_by={searchParameters.FacetBy}";
        if (searchParameters.MaxFacetValues is not null)
            urlParameters += $"&max_facet_values={searchParameters.MaxFacetValues}";
        if (searchParameters.FacetQuery is not null)
            urlParameters += $"&facet_query={searchParameters.FacetQuery}";
        if (searchParameters.NumberOfTypos is not null)
            urlParameters += $"&num_typos={searchParameters.NumberOfTypos}";
        if (searchParameters.Page is not null)
            urlParameters += $"&page={searchParameters.Page}";
        if (searchParameters.PerPage is not null)
            urlParameters += $"&per_page={searchParameters.PerPage}";
        if (searchParameters.IncludeFields is not null)
            urlParameters += $"&include_fields={searchParameters.IncludeFields}";
        if (searchParameters.HighlightFullFields is not null)
            urlParameters += $"&highlight_full_fields={searchParameters.HighlightFullFields}";
        if (searchParameters.HighlightAffixNumberOfTokens is not null)
            urlParameters += $"&highlight_affix_num_tokens={searchParameters.HighlightAffixNumberOfTokens}";
        if (searchParameters.HighlightStartTag is not null)
            urlParameters += $"&highlight_start_tag={searchParameters.HighlightStartTag}";
        if (searchParameters.HighlightEndTag is not null)
            urlParameters += $"&highlight_end_tag={searchParameters.HighlightEndTag}";
        if (searchParameters.SnippetThreshold is not null)
            urlParameters += $"&snippet_threshold={searchParameters.SnippetThreshold}";
        if (searchParameters.DropTokensThreshold is not null)
            urlParameters += $"&drop_tokens_threshold={searchParameters.DropTokensThreshold}";
        if (searchParameters.TypoTokensThreshold is not null)
            urlParameters += $"&typo_tokens_threshold={searchParameters.TypoTokensThreshold}";
        if (searchParameters.PinnedHits is not null)
            urlParameters += $"&pinned_hits={searchParameters.PinnedHits}";
        if (searchParameters.HiddenHits is not null)
            urlParameters += $"&hidden_hits={searchParameters.HiddenHits}";
        if (searchParameters.LimitHits is not null)
            urlParameters += $"&limit_hits={searchParameters.LimitHits}";
        if (searchParameters.PreSegmentedQuery is not null)
            urlParameters += $"&pre_segmented_query={searchParameters.PreSegmentedQuery.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.EnableOverrides is not null)
            urlParameters += $"&enable_overrides={searchParameters.EnableOverrides.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.SplitJoinTokens is not null)
            urlParameters += $"&split_join_tokens={searchParameters.SplitJoinTokens.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.MaxCandiates is not null)
            urlParameters += $"&max_candidates={searchParameters.MaxCandiates.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.FacetQueryNumberTypos is not null)
            urlParameters += $"&facet_query_num_typos={searchParameters.FacetQueryNumberTypos.Value.ToString().ToLowerInvariant()}";
        if (searchParameters.Infix is not null)
            urlParameters += $"&infix={searchParameters.Infix}";

        if (searchParameters is GroupedSearchParameters groupedSearchParameters)
        {
            urlParameters += $"&group_by={groupedSearchParameters.GroupBy}";
            if (groupedSearchParameters.GroupLimit is not null)
            {
                urlParameters += $"&group_limit={groupedSearchParameters.GroupLimit}";
            }
        }


        return urlParameters;
    }

    private async Task<string> Get(string path)
    {
        var (response, responseString) = await HandleHttpResponse(_httpClient.GetAsync, path).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Delete(string path)
    {
        var (response, responseString) = await HandleHttpResponse(_httpClient.DeleteAsync, path).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Post(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        using var stringContent = GetApplicationJsonStringContent(jsonString);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PostAsync, path, stringContent).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Patch(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        using var stringContent = GetApplicationJsonStringContent(jsonString);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PatchAsync, path, stringContent).ConfigureAwait(false);
        return response.IsSuccessStatusCode
            ? responseString
            : throw GetException(response.StatusCode, responseString);
    }

    private async Task<string> Put(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        using var stringContent = GetApplicationJsonStringContent(jsonString);
        var (response, responseString) = await HandleHttpResponse(_httpClient.PutAsync, path, stringContent).ConfigureAwait(false);
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
        Func<string, Task<HttpResponseMessage>> f, string path)
    {
        var response = await f(path).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return (response, responseString);
    }

    private static async Task<(HttpResponseMessage response, string responseString)> HandleHttpResponse(
        Func<string, HttpContent, Task<HttpResponseMessage>> f, string path, StringContent stringContent)
    {
        var response = await f(path, stringContent).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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

    private static string CreateJsonNewlines<T>(IEnumerable<T> documents, JsonSerializerOptions jsonOptions)
        => String.Join('\n', documents.Select(x => JsonSerializer.Serialize(x, jsonOptions)));

    private SearchResult<T> HandleDeserializeMultiSearch<T>(JsonElement jsonElement)
        => jsonElement.Deserialize<SearchResult<T>>(_jsonNameCaseInsentiveTrue)
        ?? throw new InvalidOperationException($"Could not deserialize {typeof(T)}, Received following from Typesense: '{jsonElement}'.");
}
