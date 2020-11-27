using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Typesense;
using Typesense.Setup;

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

            var houseOne = new House
            {
                Id = "1",
                HouseNumber = 2,
                AccesAddress = "Smedgade 25B"
            };

            var houseTwo = new House
            {
                Id = "2",
                HouseNumber = 66,
                AccesAddress = "Smedgade 67B"
            };

            var houseThree = new House
            {
                Id = "3",
                HouseNumber = 33,
                AccesAddress = "Medad 55A"
            };

            var houseFour = new House
            {
                Id = "4",
                HouseNumber = 3,
                AccesAddress = "Daramed"
            };

            var query = new SearchParameters();
            query.Text = "da";
            query.QueryBy = "accesAdress";

            var schema = new Schema();
            schema.Name = "Adresses";
            schema.Fields = new List<Field>();
            schema.Fields.Add(new Field("id", "string", false));
            schema.Fields.Add(new Field("houseNumber", "int32", false));
            schema.Fields.Add(new Field("accesAdress", "string", false));
            schema.DefaultSortingField = "houseNumber";

            await typesenseClient.CreateCollection(schema);
            await typesenseClient.RetrieveCollections();
            await typesenseClient.CreateDocument("Adresses", houseOne);
            await typesenseClient.CreateDocument("Adresses", houseTwo);
            await typesenseClient.CreateDocument("Adresses", houseThree);
            await typesenseClient.CreateDocument("Adresses", houseFour);
            await typesenseClient.RetrieveCollection("Adresses");
            await typesenseClient.Search("Adresses", query);

        }
    }
}
