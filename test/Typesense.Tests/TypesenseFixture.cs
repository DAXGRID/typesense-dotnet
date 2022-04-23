using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Typesense.Setup;
using Xunit;

namespace Typesense.Tests;

public class TypesenseFixture : IAsyncLifetime
{
    public ITypesenseClient Client => GetClient();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            CleanCollections(),
            CleanApiKeys(),
            CleanAlias());
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

    private async Task CleanAlias()
    {
        var aliases = await Client.ListCollectionAliases();
        foreach (var alias in aliases.CollectionAliases)
        {
            await Client.DeleteCollectionAlias(alias.Name);
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
                    new Node("localhost", "8108", "http")
                };
            }).BuildServiceProvider().GetService<ITypesenseClient>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
