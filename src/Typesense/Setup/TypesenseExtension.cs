using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

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
            .AddHttpClient<ITypesenseClient, TypesenseClient>().Services
            .Configure(config);
    }

    public static IServiceCollection AddTypesenseClientWithCustomSerializer(this IServiceCollection serviceCollection, Action<Config> config, JsonSerializerOptions serializerOptions)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), $"Please provide options for TypesenseClient.");

        return serviceCollection
            .AddScoped<ITypesenseClient, TypesenseClient>()
            .AddScoped<JsonSerializerOptions>(x => serializerOptions)
            .AddHttpClient<ITypesenseClient, TypesenseClient>().Services
            .Configure(config);
    }
}
