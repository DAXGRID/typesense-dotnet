using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace Typesense.Setup;

public static class TypesenseExtension
{
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddTypesenseClient(this IServiceCollection serviceCollection, Action<Config> config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), $"Please provide options for TypesenseClient.");

        return serviceCollection
            .AddScoped<ITypesenseClient, TypesenseClient>()
            .AddHttpClient<ITypesenseClient, TypesenseClient>(client =>
            {
                client.DefaultRequestVersion = HttpVersion.Version30;
            }).Services
            .Configure(config);
    }
}
