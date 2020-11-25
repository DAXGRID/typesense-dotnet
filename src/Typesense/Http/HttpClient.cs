using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Typesense.Http
{
    internal class HttpClient
    {

        public async Task<string> Post(string uri, string json, string apiKey)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-TYPESENSE-API-KEY", apiKey);

                var result = await httpClient.PostAsync(uri, new StringContent(json));
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
