using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Typesense.Setup;

namespace Typesense;
public class TypesenseClient : ITypesenseClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonNameCaseInsentiveTrue = new() { PropertyNameCaseInsensitive = true };

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

        var response = await Post($"/collections", schema);
        return JsonSerializer.Deserialize<CollectionResponse>(response);
    }

    public async Task<T> CreateDocument<T>(string collection, T document)
    {
        if (collection is null || document is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");

        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var response = await Post($"/collections/{collection}/documents", document);
        return JsonSerializer.Deserialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpsertDocument<T>(string collection, T document)
    {
        if (collection is null || document is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");

        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var response = await Post($"/collections/{collection}/documents?action=upsert", document);

        if (string.IsNullOrEmpty(response))
            return default(T);

        return JsonSerializer.Deserialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters)
    {
        if (collection is null || searchParameters is null)
            throw new ArgumentNullException($"{nameof(collection)} or {nameof(searchParameters)} cannot be null.");

        if (collection == string.Empty)
            throw new ArgumentException($"{nameof(collection)} cannot be empty");

        var parameters = CreateUrlSearchParameters(searchParameters);
        var response = await Get($"/collections/{collection}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}");
        return JsonSerializer.Deserialize<SearchResult<T>>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> RetrieveDocument<T>(string collection, string id)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");

        var response = await Get($"/collections/{collection}/documents/{id}");

        if (string.IsNullOrEmpty(response))
            return default(T);

        return JsonSerializer.Deserialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<T> UpdateDocument<T>(string collection, string id, T document)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");

        if (document is null)
            throw new ArgumentNullException($"{nameof(document)} cannot be null");

        var response = await Patch($"collections/{collection}/documents/{id}", document);

        if (string.IsNullOrEmpty(response))
            return default(T);

        return JsonSerializer.Deserialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<Collection> RetrieveCollection(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty.");

        var response = await Get($"/collections/{name}");

        if (string.IsNullOrEmpty(response))
            return null;

        return JsonSerializer.Deserialize<Collection>(response);
    }

    public async Task<IReadOnlyCollection<Collection>> RetrieveCollections()
    {
        var response = await Get($"/collections");
        return JsonSerializer.Deserialize<IReadOnlyCollection<Collection>>(response);
    }

    public async Task<T> DeleteDocument<T>(string collection, string documentId)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(documentId))
            throw new ArgumentException($"{nameof(collection)} or {nameof(documentId)} cannot be null or empty.");

        var response = await Delete($"/collections/{collection}/documents/{documentId}");

        if (string.IsNullOrEmpty(response))
            return default(T);

        return JsonSerializer.Deserialize<T>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(filter))
            throw new ArgumentException($"{nameof(collection)} or {nameof(filter)} cannot be null or empty.");

        var response = await Delete($"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}");
        return JsonSerializer.Deserialize<FilterDeleteResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionResponse> DeleteCollection(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty");

        var response = await Delete($"/collections/{name}");

        if (string.IsNullOrEmpty(response))
            return null;

        return JsonSerializer.Deserialize<CollectionResponse>(response);
    }

    public async Task<IReadOnlyCollection<ImportResponse>> ImportDocuments<T>(string collection, List<T> documents, int batchSize = 40, ImportType importType = ImportType.Create)
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

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var jsonString = new StringBuilder();
        foreach (var document in documents)
        {
            var json = JsonSerializer.Serialize(document, jsonOptions);
            jsonString.Append(json + '\n');
        }

        var response = await _httpClient.PostAsync(path, new StringContent(jsonString.ToString(), Encoding.UTF8, "text/plain"));

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        var responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());

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
        var response = await Get($"/collections/{collection}/documents/export?{searchParameters}");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return response.Split('\n').Select((x) => JsonSerializer.Deserialize<T>(x, jsonOptions)).ToList();
    }

    public async Task<KeyResponse> CreateKey(Key key)
    {
        if (key is null)
            throw new ArgumentNullException($"{nameof(key)} or {nameof(key)} cannot be null.");

        var response = await Post($"/keys", key);
        return JsonSerializer.Deserialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<KeyResponse> RetrieveKey(int id)
    {
        var response = await Get($"/keys/{id}");

        if (string.IsNullOrEmpty(response))
            return default(KeyResponse);

        return JsonSerializer.Deserialize<KeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteKeyResponse> DeleteKey(int id)
    {
        var response = await Delete($"/keys/{id}");

        if (string.IsNullOrEmpty(response))
            return default(DeleteKeyResponse);

        return JsonSerializer.Deserialize<DeleteKeyResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListKeysResponse> ListKeys()
    {
        var response = await Get($"/keys");

        if (string.IsNullOrEmpty(response))
            return default(ListKeysResponse);

        return JsonSerializer.Deserialize<ListKeysResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchOverride> UpsertSearchOverride(string collection, string overrideName, SearchOverride searchOverride)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");
        if (searchOverride is null)
            throw new ArgumentNullException($"{nameof(searchOverride)} cannot be null.");

        var response = await Put($"/collections/{collection}/overrides/{overrideName}", searchOverride);

        if (string.IsNullOrEmpty(response))
            return default(SearchOverride);

        return JsonSerializer.Deserialize<SearchOverride>(response, _jsonNameCaseInsentiveTrue);

    }

    public async Task<ListSearchOverridesResponse> ListSearchOverrides(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"collections/{collection}/overrides");

        if (string.IsNullOrEmpty(response))
            return default(ListSearchOverridesResponse);

        return JsonSerializer.Deserialize<ListSearchOverridesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SearchOverride> RetrieveSearchOverride(string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/overrides/{overrideName}");

        if (string.IsNullOrEmpty(response))
            return default(SearchOverride);

        return JsonSerializer.Deserialize<SearchOverride>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteSearchOverrideResponse> DeleteSearchOverride(string collection, string overrideName)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(overrideName))
            throw new ArgumentException($"{nameof(overrideName)} cannot be null, empty or whitespace.");

        var response = await Delete($"/collections/{collection}/overrides/{overrideName}");

        if (string.IsNullOrEmpty(response))
            return default(DeleteSearchOverrideResponse);

        return JsonSerializer.Deserialize<DeleteSearchOverrideResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> UpsertCollectionAlias(string alias, CollectionAlias collectionAlias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException($"{nameof(alias)} cannot be null, empty or whitespace.");
        if (collectionAlias is null)
            throw new ArgumentNullException($"{nameof(collectionAlias)} cannot be null.");

        var response = await Put($"/aliases/{alias}", collectionAlias);

        if (string.IsNullOrWhiteSpace(response))
            return default(CollectionAlias);

        return JsonSerializer.Deserialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> RetrieveCollectionAlias(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"/aliases/{collection}");

        if (string.IsNullOrWhiteSpace(response))
            return default(CollectionAlias);

        return JsonSerializer.Deserialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListCollectionAliasesResponse> ListCollectionAliases()
    {
        var response = await Get("/aliases");

        if (string.IsNullOrWhiteSpace(response))
            return default(ListCollectionAliasesResponse);

        return JsonSerializer.Deserialize<ListCollectionAliasesResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<CollectionAlias> DeleteCollectionAlias(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Delete($"/aliases/{collection}");

        if (string.IsNullOrWhiteSpace(response))
            return default(CollectionAlias);

        return JsonSerializer.Deserialize<CollectionAlias>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> UpsertSynonym(string collection, string synonym, SynonymSchema schema)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");
        if (schema is null)
            throw new ArgumentException($"{nameof(schema)} cannot be null.");

        var response = await Put($"/collections/{collection}/synonyms/{synonym}", schema);

        return JsonSerializer.Deserialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<SynonymSchemaResponse> RetrieveSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms/{synonym}");

        if (string.IsNullOrWhiteSpace(response))
            return default(SynonymSchemaResponse);

        return JsonSerializer.Deserialize<SynonymSchemaResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<ListSynonymsResponse> ListSynonyms(string collection)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");

        var response = await Get($"/collections/{collection}/synonyms");

        if (string.IsNullOrWhiteSpace(response))
            return default(ListSynonymsResponse);

        return JsonSerializer.Deserialize<ListSynonymsResponse>(response, _jsonNameCaseInsentiveTrue);
    }

    public async Task<DeleteSynonymResponse> DeleteSynonym(string collection, string synonym)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException($"{nameof(collection)} cannot be null, empty or whitespace.");
        if (string.IsNullOrWhiteSpace(synonym))
            throw new ArgumentException($"{nameof(synonym)} cannot be null, empty or whitespace.");

        var response = await Delete($"/collections/{collection}/synonyms/{synonym}");

        if (string.IsNullOrWhiteSpace(response))
            return default(DeleteSynonymResponse);

        return JsonSerializer.Deserialize<DeleteSynonymResponse>(response, _jsonNameCaseInsentiveTrue);
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

    private async Task<string> Post(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        var response = await _httpClient.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> Get(string path)
    {
        var response = await _httpClient.GetAsync(path);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        if (!response.IsSuccessStatusCode)
            throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> Delete(string path)
    {
        var response = await _httpClient.DeleteAsync(path);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        if (!response.IsSuccessStatusCode)
            throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> Patch(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var response = await _httpClient.PatchAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

        if (!response.IsSuccessStatusCode)
            throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> Put(string path, object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var response = await _httpClient.PutAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

        return await response.Content.ReadAsStringAsync();
    }
}
