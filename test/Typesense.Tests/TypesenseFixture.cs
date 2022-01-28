using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Typesense.Setup;

namespace Typesense.Tests;

public class TypesenseFixture
{
    public ITypesenseClient Client => GetClient();

    public TypesenseFixture()
    {
        Cleanup().ContinueWith(_ => { });
    }

    private async Task Cleanup()
    {
        var collections = await Client.RetrieveCollections();

        foreach (var collection in collections)
        {
            await Client.DeleteCollection(collection.Name);
        }
    }

    private ITypesenseClient GetClient()
    {
        return new ServiceCollection()
            .AddTypesenseClient(config =>
            {
                config.ApiKey = "key";
                config.Nodes = new List<Node> { new Node { Host = "localhost", Port = "8108", Protocol = "http" } };
            }).BuildServiceProvider().GetService<ITypesenseClient>();
    }
}
