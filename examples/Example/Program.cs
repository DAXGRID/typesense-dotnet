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

            var schema = new Schema
            {
                Name = "Addresses",
                Fields = new List<Field>
                {
                    new Field("id", "string", false),
                    new Field("houseNumber", "int32", false),
                    new Field("accessAddress", "string", false),
                },
                DefaultSortingField = "houseNumber"
            };

            await typesenseClient.CreateCollection(schema);

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

            await typesenseClient.CreateDocument("Adresses", houseOne);
            await typesenseClient.CreateDocument("Adresses", houseTwo);
            await typesenseClient.CreateDocument("Adresses", houseThree);
            await typesenseClient.CreateDocument("Adresses", houseFour);

            var query = new SearchParameters
            {
                Text = "da",
                QueryBy = "accessAddress"
            };

            await typesenseClient.Search("Adresses", query);
        }
    }
}
