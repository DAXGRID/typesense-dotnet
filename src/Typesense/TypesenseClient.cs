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
        httpClient.BaseAddress = new Uri($"{config.Value.Nodes[0].Protocol}://{config.Value.Nodes[0].Host}:{config.Value.Nodes[0].Port}");
        httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", config.Value.ApiKey);
        _httpClient = httpClient;
    }

    public async Task<CollectionResponse> CreateCollection(Schema schema)
    {
        if (schema is null)
            throw new ArgumentNullException($"The supplied argument {nameof(Schema)} cannot be null");

        var response = await Post($"/collections", schema).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response);
    }

    public async Task<T> CreateDocument<T>(string collection, T document) where T : class
    {
        if (collection is null || document is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");
        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var response = await Post($"/collections/{collection}/documents", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpsertDocument<T>(string collection, T document) where T : class
    {
        if (collection is null || document is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");
        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var response = await Post($"/collections/{collection}/documents?action=upsert", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters)
    {
        if (collection is null || searchParameters is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(searchParameters)} cannot be null.");
        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var parameters = CreateUrlSearchParameters(searchParameters);
        var response = await Get($"/collections/{collection}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchResult<T>>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> RetrieveDocument<T>(string collection, string id) where T : class
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");

        var response = await Get($"/collections/{collection}/documents/{id}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document) where T : class
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");
        if (document is null)
            throw new ArgumentNullException($"{nameof(document)} cannot be null");

        var response = await Patch($"collections/{collection}/documents/{id}", document).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> RetrieveCollection(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty.");

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
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(documentId))
            throw new ArgumentException($"{nameof(collection)} or {nameof(documentId)} cannot be null or empty.");

        var response = await Delete($"/collections/{collection}/documents/{documentId}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(filter))
            throw new ArgumentException($"{nameof(collection)} or {nameof(filter)} cannot be null or empty.");

        var response = await Delete($"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<FilterDeleteResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty");

        var response = await Delete($"/collections/{name}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<List<ImportResponse>> ImportDocuments<T>(
        string collection, List<T> documents, int batchSize = 40, ImportType importType = ImportType.Create)
    {
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null or empty");
        if (documents is null)
            throw new ArgumentNullException($"{nameof(documents)} cannot be null.");

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
        }

        var jsonNewlines = String.Join('\n', documents.Select(x => JsonSerializer.Serialize(x, _jsonOptionsCamelCaseIgnoreWritingNull)));
        var response = await _httpClient.PostAsync(path, GetTextPlainStringContent(jsonNewlines)).ConfigureAwait(false);
        var responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));

        if (!response.IsSuccessStatusCode)
            throw new Exception(responseString);

        return responseString.Split('\n').Select((x) => JsonSerializer.Deserialize<ImportResponse>(x)).ToList();
    }

    public async Task<List<T>> ExportDocuments<T>(string collection)
    {
        return await ExportDocuments<T>(collection, new ExportParameters());
    }

    public async Task<List<T>> ExportDocuments<T>(string collection, ExportParameters exportParameters)
    {
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null or empty.");
        if (exportParameters is null)
            throw new ArgumentNullException($"{nameof(exportParameters)} cannot be null.");

        var extraParameters = new List<string>();
        if (exportParameters.IncludeFields is not null)
            extraParameters.Add($"include_fields={exportParameters.ExcludeFields}");
        if (exportParameters.FilterBy is not null)
            extraParameters.Add($"filter_by={exportParameters.FilterBy}");
        if (exportParameters.ExcludeFields is not null)
            extraParameters.Add($"exclude_fields={exportParameters.ExcludeFields}");

        var searchParameters = string.Join("&", extraParameters);
        var response = await Get($"/collections/{collection}/documents/export?{searchParameters}").ConfigureAwait(false);

        return response.Split('\n').Select((x) => JsonSerializer.Deserialize<T>(x, _jsonNameCaseInsentiveTrue)).ToList();
    }

    public async Task<KeyResponse> CreateKey(Key key)
    {
        if (key is null)
            throw new ArgumentNullException($"{nameof(key)} or {nameof(key)} cannot be null.");

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
        var securityKeyAsBuffer = Encoding.UTF8.GetBytes(securityKey);
        var parametersAsBuffer = Encoding.UTF8.GetBytes(parameters);

        using var hmac = new HMACSHA256(securityKeyAsBuffer);

        var hash = hmac.ComputeHash(parametersAsBuffer);
        var digest = Convert.ToBase64String(hash);
        var keyPrefix = securityKey[..4];
        var rawScopedKey = $"{digest}{keyPrefix}{parameters}";

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawScopedKey));
    }

    public async Task<SearchOverride> UpsertSearchOverride(
        string collection, string overrideName, SearchOverride searchOverride)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");
        if (searchOverride is null)
            throw new ArgumentNullException($"{nameof(searchOverride)} cannot be null.");

        var response = await Put($"/collections/{collection}/overrides/{overrideName}", searchOverride).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchOverride>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSearchOverridesResponse> ListSearchOverrides(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"collections/{collection}/overrides").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListSearchOverridesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchOverride> RetrieveSearchOverride(string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/overrides/{overrideName}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<SearchOverride>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteSearchOverrideResponse> DeleteSearchOverride(
        string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");

        var response = await Delete($"/collections/{collection}/overrides/{overrideName}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<DeleteSearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> UpsertCollectionAlias(string alias, CollectionAlias collectionAlias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException($"{nameof(alias)} cannot be null, empty or whitespace.");
        if (collectionAlias is null)
            throw new ArgumentNullException($"{nameof(collectionAlias)} cannot be null.");

        var response = await Put($"/aliases/{alias}", collectionAlias).ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> RetrieveCollectionAlias(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"/aliases/{collection}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListCollectionAliasesResponse> ListCollectionAliases()
    {
        var response = await Get("/aliases").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<ListCollectionAliasesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> DeleteCollectionAlias(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Delete($"/aliases/{collection}").ConfigureAwait(false);
        return HandleEmptyStringJsonSerialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> UpsertSynonym(
        string collection, string synonym, SynonymSchema schema)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");
        if (schema is null)
            throw new ArgumentException($"{nameof(schema)} cannot be null.");

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

    private string CreateUrlSearchParameters(SearchParameters searchParameters)
    {
        var builder = new StringBuilder();
        if (searchParameters.MaxHits is not null)
            builder.Append($"&max_hits={searchParameters.MaxHits}");
        if (searchParameters.QueryByWeights is not null)
            builder.Append($"&query_by_weights={searchParameters.QueryByWeights}");
        if (searchParameters.Prefix is not null)
            builder.Append($"&prefix={searchParameters.Prefix.Value.ToString().ToLower()}");
        if (searchParameters.FilterBy is not null)
            builder.Append($"&filter_by={searchParameters.FilterBy}");
        if (searchParameters.SortBy is not null)
            builder.Append($"&sort_by={searchParameters.SortBy}");
        if (searchParameters.FacetBy is not null)
            builder.Append($"&facet_by={searchParameters.FacetBy}");
        if (searchParameters.MaxFacetValues is not null)
            builder.Append($"&max_facet_values={searchParameters.MaxFacetValues}");
        if (searchParameters.FacetQuery is not null)
            builder.Append($"&facet_query={searchParameters.FacetQuery}");
        if (searchParameters.NumberOfTypos is not null)
            builder.Append($"&num_typos={searchParameters.NumberOfTypos}");
        if (searchParameters.Page is not null)
            builder.Append($"&page={searchParameters.Page}");
        if (searchParameters.PerPage is not null)
            builder.Append($"&per_page={searchParameters.PerPage}");
        if (searchParameters.GroupBy is not null)
            builder.Append($"&group_by={searchParameters.GroupBy}");
        if (searchParameters.GroupLimit is not null)
            builder.Append($"&group_limit={searchParameters.GroupLimit}");
        if (searchParameters.IncludeFields is not null)
            builder.Append($"&include_fields={searchParameters.IncludeFields}");
        if (searchParameters.HighlightFullFields is not null)
            builder.Append($"&highlight_full_fields={searchParameters.HighlightFullFields}");
        if (searchParameters.HighlightAffixNumberOfTokens is not null)
            builder.Append($"&highlight_affix_num_tokens={searchParameters.HighlightAffixNumberOfTokens}");
        if (searchParameters.HighlightStartTag is not null)
            builder.Append($"&highlight_start_tag={searchParameters.HighlightStartTag}");
        if (searchParameters.HighlightEndTag is not null)
            builder.Append($"&highlight_end_tag={searchParameters.HighlightEndTag}");
        if (searchParameters.SnippetThreshold is not null)
            builder.Append($"&snippet_threshold={searchParameters.SnippetThreshold}");
        if (searchParameters.DropTokensThreshold is not null)
            builder.Append($"&drop_tokens_threshold={searchParameters.DropTokensThreshold}");
        if (searchParameters.TypoTokensThreshold is not null)
            builder.Append($"&typo_tokens_threshold={searchParameters.TypoTokensThreshold}");
        if (searchParameters.PinnedHits is not null)
            builder.Append($"&pinned_hits={searchParameters.PinnedHits}");
        if (searchParameters.HiddenHits is not null)
            builder.Append($"&hidden_hits={searchParameters.HiddenHits}");
        if (searchParameters.LimitHits is not null)
            builder.Append($"&limit_hits={searchParameters.LimitHits}");
        if (searchParameters.PreSegmentedQuery is not null)
            builder.Append($"&pre_segmented_query={searchParameters.PreSegmentedQuery.Value.ToString().ToLower()}");
        if (searchParameters.EnableOverrides is not null)
            builder.Append($"&enable_overrides={searchParameters.EnableOverrides.Value.ToString().ToLower()}");

        return builder.ToString();
    }

    private async Task<string> Get(string path)
    {
        var response = await _httpClient.GetAsync(path).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? responseString
            : throw new TypesenseApiException(responseString);
    }

    private async Task<string> Delete(string path)
    {
        var response = await _httpClient.DeleteAsync(path).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? responseString
            : throw new TypesenseApiException(responseString);
    }

    private async Task<string> Post(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await _httpClient.PostAsync(path, GetApplicationJsonStringContent(jsonString)).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? responseString
            : throw new TypesenseApiException(responseString);
    }

    private async Task<string> Patch(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await _httpClient.PatchAsync(path, GetApplicationJsonStringContent(jsonString)).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? responseString
            : throw new TypesenseApiException(responseString);
    }

    private async Task<string> Put(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), _jsonOptionsCamelCaseIgnoreWritingNull);
        var response = await _httpClient.PutAsync(path, GetApplicationJsonStringContent(jsonString)).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? responseString
            : throw new TypesenseApiException(responseString);
    }

    private static StringContent GetApplicationJsonStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "application/json");

    private static StringContent GetTextPlainStringContent(string jsonString)
        => new(jsonString, Encoding.UTF8, "text/plain");

    private static T HandleEmptyStringJsonSerialize<T>(
        string json, JsonSerializerOptions options = null) where T : class
        => !string.IsNullOrEmpty(json)
               ? JsonSerializer.Deserialize<T>(json, options)
               : null;
}
