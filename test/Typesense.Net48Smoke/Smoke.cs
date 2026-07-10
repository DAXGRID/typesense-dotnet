using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Typesense.Setup;

namespace Typesense.Net48Smoke;

public static class Smoke
{
    public static ITypesenseClient? ConfigureAndResolveClient()
    {
        var services = new ServiceCollection();

        services.AddTypesenseClient(config =>
        {
            config.ApiKey = "xyz";
            config.Nodes = new List<Node>
            {
                new("localhost", "8108", "http", "")
            };
        });

        using var provider = services.BuildServiceProvider();
        return provider.GetService<ITypesenseClient>();
    }
}
