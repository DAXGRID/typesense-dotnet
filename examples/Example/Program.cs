using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Typesense;
using Typesense.Setup;
using System;

namespace Example
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddTypesenseClient(config =>
                {
                    config.ApiKey = "Hu52dwsas2AdxdE";
                    config.Nodes = new List<Node> { new Node { Host = "localhost", Port = "8108", Protocol = "http" } };
                }).BuildServiceProvider();

            var typesenseClient = provider.GetService<ITypesenseClient>();
            var schema = new Schema();
            schema.Name = "Adresses";
            schema.Fields =  new List<Field>();
            schema.Fields.Add(new Field("id","string",false));
            schema.Fields.Add(new Field("houseNumber","int32",false));
            schema.Fields.Add(new Field("accesAdress","string",false));
            schema.DefaultSortingField = "houseNumber";
           

            await typesenseClient.CreateCollection(schema);

        }
    }
}
