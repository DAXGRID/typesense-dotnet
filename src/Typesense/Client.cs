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

        public async Task<CollectionResponse> CreateCollection(Schema schema)
        {
            var response = await Post($"/collections", schema);
            return JsonSerializer.Deserialize<CollectionResponse>(response);
        }

        public async Task<T> CreateDocument<T>(string collection, object document)
        {
            var response = await Post($"/collections/{collection}/documents", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> UpsertDocument<T>(string collection, object document)
        {
            var response = await Post($"/collections/{collection}/documents?action=upsert", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<SearchResult<T>> Search<T>(string collection, SearchParameters searchParameters)
        {
            var parameters = CreateUrlSearchParameters(searchParameters);
            var response = await Get($"/collections/{collection}/documents/search?q={searchParameters.Text}&query_by={searchParameters.QueryBy}{parameters}");
            return JsonSerializer.Deserialize<SearchResult<T>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> RetrieveDocument<T>(string collection, string id)
        {
            var response = await Get($"/collections/{collection}/documents/{id}");
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> UpdateDocument<T>(string collection, string id, T document)
        {
            var response = await Patch($"collections/{collection}/documents/{id}", document);
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Collection> RetrieveCollection(string name)
        {
            var response = await Get($"/collections/{name}");
            return JsonSerializer.Deserialize<Collection>(response);
        }

        public async Task RetrieveCollections()
        {
            await Get($"/collections");
        }

        public async Task<T> DeleteDocument<T>(string collection, string documentId)
        {
            var response = await Delete($"/collections/{collection}/documents/{documentId}");
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FilterDeleteResponse> DeleteDocuments(string collection, string filter, int batchSize)
        {
            var response = await Delete($"/collections/{collection}/documents?filter_by={filter}&batch_size={batchSize}");
            return JsonSerializer.Deserialize<FilterDeleteResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<CollectionResponse> DeleteCollection(string name)
        {
            var response = await Delete($"/collections/{name}");
            return JsonSerializer.Deserialize<CollectionResponse>(response);
        }

        public async Task<IReadOnlyCollection<ImportResponse>> ImportDocuments<T>(string collection, List<T> documents, int batchSize = 40, ImportType importType = ImportType.Create)
        {
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

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonString = new StringBuilder();

            foreach (var document in documents)
            {
                var json = JsonSerializer.Serialize(document, jsonOptions);
                jsonString.Append(json + Environment.NewLine);
            }

            var response = await _httpClient.PostAsync(path, new StringContent(jsonString.ToString(), Encoding.UTF8, "text/plain"));
            var responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());

            return responseString.Split(Environment.NewLine).Select((x) => JsonSerializer.Deserialize<ImportResponse>(x)).ToList();
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

        private async Task<string> Patch(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = await _httpClient.PatchAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }


    }
}
