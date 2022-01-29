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
        var cleanCollectionsTask = CleanCollections();
        var cleanApiKeysTask = CleanApiKeys();
        await Task.WhenAll(cleanApiKeysTask, cleanCollectionsTask);
    }

    private async Task CleanCollections()
    {
        var collections = await Client.RetrieveCollections();
        foreach (var collection in collections)
        {
            await Client.DeleteCollection(collection.Name);
        }
    }

    private async Task CleanApiKeys()
    {
        var apiKeys = await Client.ListKeys();
        foreach (var key in apiKeys.Keys)
        {
            await Client.DeleteKey(key.Id);
        }
    }

    private ITypesenseClient GetClient()
    {
        return new ServiceCollection()
            .AddTypesenseClient(config =>
            {
                config.ApiKey = "key";
                config.Nodes = new List<Node>
                {
                    new Node
                    {
                        Host = "localhost",
                        Port = "8108",
                        Protocol = "http"
                    }
                };
            }).BuildServiceProvider().GetService<ITypesenseClient>();
    }
}
