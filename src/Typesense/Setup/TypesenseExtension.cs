using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;

namespace Typesense.Setup;

public static class TypesenseExtension
{
    /// <param name="serviceCollection">
    /// The collection of services to be configured for the Typesense client.
    /// </param>
    /// <param name="config">
    /// The configuration for the Typesense client.
    /// </param>
    /// <param name="enableHttpCompression">
    /// If set to true, HTTP compression is enabled, lowering response times & reducing traffic for externally hosted Typesense, like Typesense Cloud
    /// Set to false by default to mimic the old behavior, and not add compression processing overhead on locally hosted Typesense
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddTypesenseClient(this IServiceCollection serviceCollection, Action<Config> config, bool enableHttpCompression = false)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), $"Please provide options for TypesenseClient.");

        var httpClientBuilder = serviceCollection
            .AddScoped<ITypesenseClient, TypesenseClient>()
            .AddHttpClient<ITypesenseClient, TypesenseClient>(client =>
            {
                client.DefaultRequestVersion = HttpVersion.Version30;
            });
        if (enableHttpCompression)
            httpClientBuilder = httpClientBuilder
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                });
        return httpClientBuilder.Services
            .Configure(config);
    }
}
