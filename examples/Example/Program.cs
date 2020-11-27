using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Typesense;
using Typesense.Setup;

namespace Example
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddTypesenseClient(config =>
                {
                    config.ApiKey = "dsfsfs";
                    config.Nodes = new List<Node> { new Node { Host = "", Port = "2222", Protocol = "http" } };
                }).BuildServiceProvider();

            var typesenseClient = provider.GetService<ITypesenseClient>();
            await typesenseClient.CreateCollection(new Schema());
        }
    }
}
