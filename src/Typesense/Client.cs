using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

        public async Task RetrieveCollection(string schema)
        {
            await Get($"/collections/{schema}");
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
            var jsonString = obj.ToString();
            var result = await _httpClient.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));
            return await result.Content.ReadAsStringAsync();
        }

        private async Task<string> Get(string path)
        {
            var result = await _httpClient.GetAsync(path);
            string responseData = await result.Content.ReadAsStringAsync();
            return await result.Content.ReadAsStringAsync();
        }
    }
}
