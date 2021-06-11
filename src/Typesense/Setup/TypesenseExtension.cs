using System;
using Microsoft.Extensions.DependencyInjection;

namespace Typesense.Setup
{
    public static class TypesenseExtension
    {
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddTypesenseClient(this IServiceCollection serviceCollection, Action<Config> config)
        {
            serviceCollection.AddScoped<ITypesenseClient, TypesenseClient>();
            serviceCollection.AddHttpClient<ITypesenseClient, TypesenseClient>();

            if (config == null)
                throw new ArgumentNullException(nameof(config), $"Please provide options for TypesenseClient.");

            serviceCollection.Configure(config);
            return serviceCollection;
        }
    }
}
