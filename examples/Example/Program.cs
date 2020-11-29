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

            var createCollectionResponse = await typesenseClient.CreateCollection(schema);
            Console.WriteLine($"Created collection: {JsonSerializer.Serialize(createCollectionResponse)}");

            var retrieveCollection = await typesenseClient.RetrieveCollection("Addresses");
            Console.WriteLine($"Retrieve collection: {JsonSerializer.Serialize(retrieveCollection)}");

            var retrieveCollections = await typesenseClient.RetrieveCollections();
            Console.WriteLine($"Retrieve collections: {JsonSerializer.Serialize(retrieveCollections)}");

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

            var houseOneResponse = await typesenseClient.CreateDocument<Address>("Addresses", houseOne);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseOneResponse)}");
            var houseTwoResponse = await typesenseClient.CreateDocument<Address>("Addresses", houseTwo);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseTwo)}");
            var houseThreeResponse = await typesenseClient.CreateDocument<Address>("Addresses", houseThree);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseThreeResponse)}");
            var houseFourResponse = await typesenseClient.CreateDocument<Address>("Addresses", houseFour);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseFourResponse)}");

            var upsertHouseOne = await typesenseClient.UpsertDocument<Address>("Addresses", houseOne);
            Console.WriteLine($"Upserted document: {JsonSerializer.Serialize(upsertHouseOne)}");

            var addresses = new List<Address>
            {
                new Address { AccessAddress = "Sulstreet 4", Id = "5", HouseNumber = 223 },
                new Address { AccessAddress = "Sulstreet 24", Id = "6", HouseNumber = 321 }
            };

            var importDocuments = await typesenseClient.ImportDocuments<Address>("Addresses", addresses, 40, ImportType.Create);
            Console.WriteLine($"Import documents: {JsonSerializer.Serialize(importDocuments)}");

            houseFour.HouseNumber = 1;
            var updateDocumentResult = await typesenseClient.UpdateDocument<Address>("Addresses", "4", houseFour);
            Console.WriteLine($"Updated document: {JsonSerializer.Serialize(updateDocumentResult)}");

            var query = new SearchParameters
            {
                Text = "Sul",
                QueryBy = "accessAddress"
            };

            var searchResult = await typesenseClient.Search<Address>("Addresses", query);
            Console.WriteLine(JsonSerializer.Serialize(searchResult));

            var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
            Console.WriteLine($"Retrieved document: {JsonSerializer.Serialize(retrievedDocument)}");

            var deleteResult = await typesenseClient.DeleteDocument<Address>("Addresses", "2");
            Console.WriteLine(JsonSerializer.Serialize(deleteResult));

            var deleteFilterResult = await typesenseClient.DeleteDocuments("Addresses", "houseNumber:>=3", 100);
            Console.WriteLine($"Deleted amount: {deleteFilterResult.NumberOfDeleted}");

            var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
            Console.WriteLine($"Deleted collection: {JsonSerializer.Serialize(deleteCollectionResult)}");
        }
    }
}
