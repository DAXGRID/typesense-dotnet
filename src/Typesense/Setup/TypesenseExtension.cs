using System;
using Microsoft.Extensions.DependencyInjection;

namespace Typesense.Setup
{
    public static class TypesenseExtension
    {
        public static IServiceCollection AddTypesenseClient(this IServiceCollection serviceCollection, Action<Config> config)
        {
            serviceCollection.AddScoped<ITypesenseClient, Client>();
            serviceCollection.AddHttpClient<ITypesenseClient, Client>();

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config),
                    @"Please provide options for MyService.");
            }

            serviceCollection.Configure(config);
            return serviceCollection;
        }
    }
}
