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

        public async Task Search(string schema, SearchParameters obj)
        {
            var builder = new StringBuilder();
            if (obj.FilterBy != null)
            {
                builder.Append($"&filter_by={obj.FilterBy}");
            }
            else if (obj.GroupBy != null)
            {
                builder.Append($"&group_by={obj.GroupBy}");
            }
            else if (obj.SortBy != null)
            {
                builder.Append($"&sort_by={obj.SortBy}");
            }
            else if (obj.GroupLimit != null)
            {
                builder.Append($"&group_limit={obj.GroupLimit}");
            }

            await Get($"/collections/{schema}/documents/search?q={obj.Text}&query_by={obj.QueryBy}{builder}");
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

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri($"{_config.Nodes[0].Protocol}://{_config.Nodes[0].Host}:{_config.Nodes[0].Port}");
            _httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", _config.ApiKey);
        }

        private async Task<string> Post(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Console.WriteLine(jsonString);
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
    }
}
