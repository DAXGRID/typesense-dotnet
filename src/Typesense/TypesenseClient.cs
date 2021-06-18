using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Typesense
{
    public class TypesenseClient : ITypesenseClient
    {
        private Config _config;
        private HttpClient _httpClient;

        public TypesenseClient(IOptions<Config> config, HttpClient httpClient)
        {
            _config = config.Value;
            _httpClient = httpClient;
            ConfigureHttpClient();
        }

        public async Task<CollectionResponse> CreateCollection(Schema schema)
        {
            if (schema is null)
                throw new ArgumentNullException($"The supplied argument {nameof(Schema)} cannot be null");

            var response = await Post($"/collections", schema);
            return JsonSerializer.Deserialize<CollectionResponse>(response);
        }

        public async Task<T> CreateDocument<T>(string collection, object document)
        {
            if (collection is null || document is null)
                throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");

            if (collection == string.Empty)
                throw new ArgumentException($"{nameof(collection)} cannot be empty");

            var response = await Post($"/collections/{collection}/documents", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> UpsertDocument<T>(string collection, object document)
        {
            if (collection is null || document is null)
                throw new ArgumentNullException($"{nameof(collection)} or {nameof(document)} cannot be null.");

            if (collection == string.Empty)
                throw new ArgumentException($"{nameof(collection)} cannot be empty");

            var response = await Post($"/collections/{collection}/documents?action=upsert", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters)
        {
            if (collection is null || searchParameters is null)
                throw new ArgumentNullException($"{nameof(collection)} or {nameof(searchParameters)} cannot be null.");

            if (collection == string.Empty)
                throw new ArgumentException($"{nameof(collection)} cannot be empty");

            var parameters = CreateUrlSearchParameters(searchParameters);
            var response = await Get($"/collections/{collection}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}");
            return JsonSerializer.Deserialize<SearchResult<T>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> RetrieveDocument<T>(string collection, string id)
        {
            if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
                throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");

            var response = await Get($"/collections/{collection}/documents/{id}");
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> UpdateDocument<T>(string collection, string id, T document)
        {
            if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
                throw new ArgumentException($"{nameof(collection)} or {nameof(id)} cannot be null or empty.");

            if (document is null)
                throw new ArgumentNullException($"{nameof(document)} cannot be null");

            var response = await Patch($"collections/{collection}/documents/{id}", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Collection> RetrieveCollection(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} cannot be null or empty.");

            var response = await Get($"/collections/{name}");
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
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize)
        {
            if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(filter))
                throw new ArgumentException($"{nameof(collection)} or {nameof(filter)} cannot be null or empty.");

            var response = await Delete($"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}");
            return JsonSerializer.Deserialize<FilterDeleteResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<CollectionResponse> DeleteCollection(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} cannot be null or empty");

            var response = await Delete($"/collections/{name}");
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

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true };
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
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException($"{nameof(collection)} cannot be null or empty.");

            var response = await Get($"/collections/{collection}/documents/export");

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return response.Split('\n').Select((x) => JsonSerializer.Deserialize<T>(x, jsonOptions)).ToList();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri($"{_config.Nodes[0].Protocol}://{_config.Nodes[0].Host}:{_config.Nodes[0].Port}");
            _httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", _config.ApiKey);
        }

        private string CreateUrlSearchParameters(SearchParameters searchParameters)
        {
            var builder = new StringBuilder();

            if (searchParameters.MaxHits != null)
                builder.Append($"&max_hits={searchParameters.MaxHits}");
            else if (searchParameters.QueryByWeights != null)
                builder.Append($"query_by_weights={searchParameters.QueryByWeights}");
            else if (searchParameters.Prefix != null)
                builder.Append($"&prefix={searchParameters.Prefix}");
            else if (searchParameters.FilterBy != null)
                builder.Append($"&filter_by={searchParameters.FilterBy}");
            else if (searchParameters.SortBy != null)
                builder.Append($"&sort_by={searchParameters.SortBy}");
            else if (searchParameters.FacetBy != null)
                builder.Append($"&facet_by={searchParameters.FacetBy}");
            else if (searchParameters.MaxFacetValues != null)
                builder.Append($"&max_facet_values={searchParameters.MaxFacetValues}");
            else if (searchParameters.FacetQuery != null)
                builder.Append($"&facet_query={searchParameters.FacetQuery}");
            else if (searchParameters.NumberOfTypos != null)
                builder.Append($"&num_typos={searchParameters.NumberOfTypos}");
            else if (searchParameters.Page != null)
                builder.Append($"&page={searchParameters.Page}");
            else if (searchParameters.PerPage != null)
                builder.Append($"&per_page={searchParameters.PerPage}");
            else if (searchParameters.GroupBy != null)
                builder.Append($"&group_by={searchParameters.GroupBy}");
            else if (searchParameters.GroupLimit != null)
                builder.Append($"&group_limit={searchParameters.GroupLimit}");
            else if (searchParameters.IncludeFields != null)
                builder.Append($"&include_fields={searchParameters.IncludeFields}");
            else if (searchParameters.HighlightFullFields != null)
                builder.Append($"&highlight_full_fields={searchParameters.HighlightFullFields}");
            else if (searchParameters.HighlightAffixNumberOfTokens != null)
                builder.Append($"&highlight_affix_num_tokens={searchParameters.HighlightAffixNumberOfTokens}");
            else if (searchParameters.HighlightStartTag != null)
                builder.Append($"&highlight_start_tag={searchParameters.HighlightStartTag}");
            else if (searchParameters.HighlightEndTag != null)
                builder.Append($"&highlight_end_tag={searchParameters.HighlightEndTag}");
            else if (searchParameters.SnippetThreshold != null)
                builder.Append($"&snippet_threshold={searchParameters.SnippetThreshold}");
            else if (searchParameters.DropTokensThreshold != null)
                builder.Append($"&drop_tokens_threshold={searchParameters.DropTokensThreshold}");
            else if (searchParameters.TypoTokensThreshold != null)
                builder.Append($"&typo_tokens_threshold={searchParameters.TypoTokensThreshold}");
            else if (searchParameters.PinnedHits != null)
                builder.Append($"&pinned_hits={searchParameters.PinnedHits}");
            else if (searchParameters.HiddenHits != null)
                builder.Append($"&hidden_hits={searchParameters.HiddenHits}");
            else if (searchParameters.LimitHits != null)
                builder.Append($"&hidden_hits={searchParameters.LimitHits}");

            return builder.ToString();
        }

        private async Task<string> Post(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true });
            var response = await _httpClient.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Get(string path)
        {
            var response = await _httpClient.GetAsync(path);

            if (!response.IsSuccessStatusCode)
                throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Delete(string path)
        {
            var response = await _httpClient.DeleteAsync(path);

            if (!response.IsSuccessStatusCode)
                throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Patch(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true });
            var response = await _httpClient.PatchAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new TypesenseApiException(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }
    }
}
