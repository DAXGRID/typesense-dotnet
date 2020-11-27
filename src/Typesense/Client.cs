using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

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
            var httpClient = new HttpClient();
            await Post($"/collections", schema);
        }

        public async Task CreateDocument(string schema, object document)
        {
            var httpClient = new HttpClient();
            await Post($"/collections/{schema}/documents", document);
        }

        public async Task Search(string schema, SearchParameters obj)
        {
           var httpClient = new HttpClient();
           await Get(@$"/collections/{schema}/documents/search?q={obj.Text}
           &query_by{obj.QueryBy}
           &filter_by={obj.FilterBy}
           &sort_by={obj.SortBy}
           &group_by{obj.GroupBy}
           &group_limit{obj.GroupLimit}"); 
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri($"{_config.Nodes[0].Host}:{_config.Nodes[0].Port}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-TYPESENSE-API-KEY", _config.ApiKey);
        }

        private async Task<string> Post(string path, object obj)
        {
            var jsonString = JsonSerializer.Serialize(obj);
            var result = await _httpClient.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));
            return await result.Content.ReadAsStringAsync();
        }

        private async Task<string> Get(string path)
        {
            var result = await _httpClient.GetAsync(path);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
