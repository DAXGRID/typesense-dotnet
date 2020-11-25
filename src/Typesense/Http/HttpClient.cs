using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace Typesense.Http
{
    internal class HttpClient
    {

        public async Task<string> Post(string uri, object obj, string apiKey)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var jsonString = JsonSerializer.Serialize(obj);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-TYPESENSE-API-KEY", apiKey);

                var result = await httpClient.PostAsync(uri, new StringContent(jsonString, Encoding.UTF8, "application/json"));
                return await result.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> Get(string uri, string apiKey)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-TYPESENSE-API-KEY", apiKey);

                var result = await httpClient.GetAsync(uri);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
