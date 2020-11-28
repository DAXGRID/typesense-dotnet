using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Typesense
{
    public class Client : ITypesenseClient
    {
        private Config _config;
        private HttpClient _httpClient;

        public Client(IOptions<Config> config, HttpClient httpClient)
        {
            _config = config.Value;
            _httpClient = httpClient;
            ConfigureHttpClient();
        }

        public async Task CreateCollection(Schema schema)
        {
            await Post($"/collections", schema);
        }

        public async Task CreateDocument(string schema, object document)
        {
            await Post($"/collections/{schema}/documents", document);
        }

        public async Task<SearchResult<T>> Search<T>(string schema, SearchParameters searchParameters)
        {
            var parameters = CreateUrlSearchParameters(searchParameters);
            var response = await Get($"/collections/{schema}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}");
            return JsonSerializer.Deserialize<SearchResult<T>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> RetrieveDocument<T>(string collection, string id)
        {
            var response = await Get($"/collections/{collection}/documents/{id}");
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Collection> RetrieveCollection(string schema)
        {
            var response = await Get($"/collections/{schema}");
            return JsonSerializer.Deserialize<Collection>(response);
        }

        public async Task RetrieveCollections()
        {
            await Get($"/collections");
        }

        public async Task<T> Delete<T>(string collection, string documentId)
        {
            var response = await Delete($"/collections/{collection}/documents/{documentId}");
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FilterDeleteResponse> Delete(string collection, string filter, int batchSize)
        {
            var response = await Delete(path: $"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}");
            return JsonSerializer.Deserialize<FilterDeleteResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri($"{_config.Nodes[0].Protocol}://{_config.Nodes[0].Host}:{_config.Nodes[0].Port}");
            _httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", _config.ApiKey);
        }

        private string CreateUrlSearchParameters(SearchParameters searchParameters)
        {
            var builder = new StringBuilder();
            if (searchParameters.FilterBy != null)
                builder.Append($"&filter_by={searchParameters.FilterBy}");
            else if (searchParameters.GroupBy != null)
                builder.Append($"&group_by={searchParameters.GroupBy}");
            else if (searchParameters.SortBy != null)
                builder.Append($"&sort_by={searchParameters.SortBy}");
            else if (searchParameters.GroupLimit != null)
                builder.Append($"&group_limit={searchParameters.GroupLimit}");

            return builder.ToString();
        }

        private async Task<string> Post(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = await _httpClient.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Get(string path)
        {
            var response = await _httpClient.GetAsync(path);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Delete(string path)
        {
            var response = await _httpClient.DeleteAsync(path);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }
    }
}
