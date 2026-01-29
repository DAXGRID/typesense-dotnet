using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Typesense.Setup;
using Xunit;

namespace Typesense.Tests;

public class TypesenseFixture : IAsyncLifetime
{
    public ITypesenseClient Client => GetClient();
    
    public Config ClientConfig = new(
        [new Node("localhost", "8108", "http")],
        "key",
        minimumCompatibilityVersion: null // Use following for Typesense v30.0: new System.Version(30, 0)
    );

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
                config.ApiKey = ClientConfig.ApiKey;
                config.Nodes = ClientConfig.Nodes;
                config.MinimumCompatibilityVersion = ClientConfig.MinimumCompatibilityVersion;
            }, enableHttpCompression: true).BuildServiceProvider().GetService<ITypesenseClient>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
