﻿using System;
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
                    config.ApiKey = "key";
                    config.Nodes = new List<Node> { new Node { Host = "localhost", Port = "8108", Protocol = "http" } };
                }).BuildServiceProvider();

            var typesenseClient = provider.GetService<ITypesenseClient>();

            var schema = new Schema
            {
                Name = "Addresses",
                Fields = new List<Field>
                {
                    new Field("id", FieldType.String, false),
                    new Field("houseNumber", FieldType.Int32, false),
                    new Field("accessAddress", FieldType.String, false, true),
                },
                DefaultSortingField = "houseNumber"
            };

            var createCollectionResponse = await typesenseClient.CreateCollection(schema);
            Console.WriteLine($"Created collection: {JsonSerializer.Serialize(createCollectionResponse)}");

            var retrieveCollection = await typesenseClient.RetrieveCollection("Addresses");
            Console.WriteLine($"Retrieve collection: {JsonSerializer.Serialize(retrieveCollection)}");

            var retrieveCollections = await typesenseClient.RetrieveCollections();
            Console.WriteLine($"Retrieve collections: {JsonSerializer.Serialize(retrieveCollections)}");

            var addressOne = new Address
            {
                HouseNumber = 2,
                AccessAddress = "Smedgade 25B"
            };

            var addressTwo = new Address
            {
                HouseNumber = 66,
                AccessAddress = "Smedgade 67B"
            };

            var addressThree = new Address
            {
                HouseNumber = 33,
                AccessAddress = "Medad 55A"
            };

            // Example to show optional AccessAddress
            var addressFour = new Address
            {
                HouseNumber = 3,
            };

            var houseOneResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressOne);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseOneResponse)}");
            var houseTwoResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressTwo);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(addressTwo)}");
            var houseThreeResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressThree);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseThreeResponse)}");
            var houseFourResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressFour);
            Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseFourResponse)}");

            var exportResult = await typesenseClient.ExportDocuments<Address>("Addresses");
            Console.WriteLine($"Export result: {JsonSerializer.Serialize(exportResult)}");

            var exportResultParameters =
                await typesenseClient.ExportDocuments<Address>("Addresses", new ExportParameters
                {
                    ExcludeFields = "houseNumber",
                });
            Console.WriteLine($"Export result: {JsonSerializer.Serialize(exportResultParameters)}");


            var upsertHouseOne = await typesenseClient.UpsertDocument<Address>("Addresses", addressOne);
            Console.WriteLine($"Upserted document: {JsonSerializer.Serialize(upsertHouseOne)}");

            var addresses = new List<Address>
            {
                new Address { AccessAddress = "Sulstreet 4", Id = "5", HouseNumber = 223 },
                new Address { AccessAddress = "Sulstreet 24", Id = "6", HouseNumber = 321 }
            };

            var importDocuments = await typesenseClient.ImportDocuments<Address>("Addresses", addresses, 40, ImportType.Create);
            Console.WriteLine($"Import documents: {JsonSerializer.Serialize(importDocuments)}");

            addressFour.HouseNumber = 1;
            var updateDocumentResult = await typesenseClient.UpdateDocument<Address>("Addresses", "4", addressFour);
            Console.WriteLine($"Updated document: ${JsonSerializer.Serialize(updateDocumentResult)}");

            var query = new SearchParameters
            {
                Text = "Sul",
                QueryBy = "accessAddress"
            };

            var searchResult = await typesenseClient.Search<Address>("Addresses", query);
            Console.WriteLine($"Search result: {JsonSerializer.Serialize(searchResult)}");

            var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
            Console.WriteLine($"Retrieved document: {JsonSerializer.Serialize(retrievedDocument)}");

            var documentNotExist = await typesenseClient.RetrieveDocument<Address>("Addresses", "1120");
            Console.WriteLine($"Retrieved document that does not exist: {JsonSerializer.Serialize(documentNotExist)}");

            var deleteResult = await typesenseClient.DeleteDocument<Address>("Addresses", "2");
            Console.WriteLine($"Deleted document {JsonSerializer.Serialize(deleteResult)}");

            var deleteFilterResult = await typesenseClient.DeleteDocuments("Addresses", "houseNumber:>=3", 100);
            Console.WriteLine($"Deleted amount: {deleteFilterResult.NumberOfDeleted}");

            var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
            Console.WriteLine($"Deleted collection: {JsonSerializer.Serialize(deleteCollectionResult)}");
        }
    }
}
