using System;
using System.Collections.Generic;
using System.Text.Json;
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

            var houseOne = new Address
            {
                Id = "1",
                HouseNumber = 2,
                AccessAddress = "Smedgade 25B"
            };

            var houseTwo = new Address
            {
                Id = "2",
                HouseNumber = 66,
                AccessAddress = "Smedgade 67B"
            };

            var houseThree = new Address
            {
                Id = "3",
                HouseNumber = 33,
                AccessAddress = "Medad 55A"
            };

            var houseFour = new Address
            {
                Id = "4",
                HouseNumber = 3,
                AccessAddress = "Daramed"
            };

            await typesenseClient.CreateDocument("Addresses", houseOne);
            await typesenseClient.CreateDocument("Addresses", houseTwo);
            await typesenseClient.CreateDocument("Addresses", houseThree);
            await typesenseClient.CreateDocument("Addresses", houseFour);

            var query = new SearchParameters
            {
                Text = "Smed",
                QueryBy = "accessAddress"
            };

            var searchResult = await typesenseClient.Search<Address>("Addresses", query);
            Console.WriteLine(JsonSerializer.Serialize(searchResult));

            var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
            Console.WriteLine($"Retrieved document: {JsonSerializer.Serialize(retrievedDocument)}");

            var deleteResult = await typesenseClient.Delete<Address>("Addresses", "2");
            Console.WriteLine(JsonSerializer.Serialize(deleteResult));

            var deleteFilterResult = await typesenseClient.Delete("Addresses", "houseNumber:>=3", 100);
            Console.WriteLine($"Deleted amount: {deleteFilterResult.NumberOfDeleted}");

            var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
            Console.WriteLine($"Deleted collection: {JsonSerializer.Serialize(deleteCollectionResult)}");
        }
    }
}
