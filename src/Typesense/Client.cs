using System.Threading.Tasks;
using Typesense.Http;

namespace Typesense
{
    public class Client : ITypesenseClient
    {
        public Config Config { get; }

        public Client(Config config)
        {
            Config = config;
        }

        public async Task CreateCollection(Schema schema)
        {
            var httpClient = new HttpClient();
            await httpClient.Post($"{Config.Nodes[0].Host}:{Config.Nodes[0].Port}/collections", schema, Config.ApiKey);
        }

        public async Task CreateDocument(string schema, object document)
        {
            var httpClient = new HttpClient();
            await httpClient.Post($"{Config.Nodes[0].Host}:{Config.Nodes[0].Port}/collections/{schema}/documents", document, Config.ApiKey);
        }
    }
}
