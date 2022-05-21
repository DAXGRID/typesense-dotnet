using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Typesense;
using Typesense.Setup;

namespace Example;

sealed class Program
{
    async static Task Main(string[] args)
    {
        // You can either create a new service collection or using an existing service collection.
        var provider = new ServiceCollection()
            .AddTypesenseClient(config =>
            {
                config.ApiKey = "key";
                config.Nodes = new List<Node> { new Node("localhost", "8108", "http") };
            }).BuildServiceProvider();

        var typesenseClient = provider.GetService<ITypesenseClient>();

        // Example of how to create an collection.
        await ExampleCreateCollection(typesenseClient);

        // Example how to retrieve a collection.
        await ExampleRetrieveCollection(typesenseClient);

        // Example how to retrieve multiple collections.
        await ExampleRetrieveCollections(typesenseClient);

        // Example of doing single inserts.
        await ExampleInsertIntoCollection(typesenseClient);

        // Example export all documents from a collection.
        await ExampleExportAllDocumentsFromCollection(typesenseClient);

        // Example exporting with export parameters from a specified collection.
        await ExampleExportDocumentsWithExportParameters(typesenseClient);

        // Example upsert document, existing and not existing.
        await ExampleUpsertDocument(typesenseClient);

        // Example importing multiple documents.
        await ExampleImportDocuments(typesenseClient);

        // Example updating existing document.
        await ExampleUpdatingExistingDocument(typesenseClient);

        // Example doing a simple search.
        await ExampleDoingSearch(typesenseClient);

        // Example showcasing retrieveing document on id.
        await ExampleRetrieveDocumentOnId(typesenseClient);

        // Example handling not found document exception, to showcase TypesenseApiException handling.
        await ExampleHandlingTypesenseApiException(typesenseClient);

        // Example creating api keys.
        await ExampleCreatingApiKeys(typesenseClient);

        // Example retrieve api key.
        await ExampleRetrieveApiKey(typesenseClient);

        // Example list all api keys.
        await ExampleListApiKeys(typesenseClient);

        // Example creating scoped api key.
        ExampleGenerateScopedApiKey(typesenseClient);

        // Example upsert search override
        await ExampleUpsertSearchOverride(typesenseClient);

        // Example list all search overrides for a given collection.
        await ExampleListAllSearchOverridesForCollection(typesenseClient);

        // Example retrieve specific search override in a specific collection.
        await ExampleRetrieveSearchOverrideInCollection(typesenseClient);

        // Example upsert collectiona alias.
        await ExampleUpsertCollectionAlias(typesenseClient);

        // Example retrieve collection alias.
        await ExampleRetrieveCollectionAlias(typesenseClient);

        // Example list all collection aliases.
        await ExampleListAllCollectionAliases(typesenseClient);

        // Example upsert synonym
        await ExampleUpsertSynonym(typesenseClient);

        // Example retrieve synonym in collection.
        await ExampleRetrieveSynonymInCollection(typesenseClient);

        // Example list all synonyms in a collection.
        await ExampleListAllSynonymsInCollection(typesenseClient);

        // Example delete collection alias
        await ExampleDeleteCollectionAlias(typesenseClient);

        // Example delete synonym
        await ExampleDeleteSynonym(typesenseClient);

        // Example delete key(s)
        foreach (var key in (await typesenseClient.ListKeys()).Keys)
            await ExampleDeleteApiKey(typesenseClient, key.Id);

        // Example delete document
        await ExampleDeleteDocumentInCollection(typesenseClient);

        // Example delete documents in colleciton with filter
        await ExampleDeleteDocumentsWithFilter(typesenseClient);

        // Example delete search override
        await ExampleDeleteSearchOverride(typesenseClient);

        // Example delete collection
        await ExampleDeleteCollection(typesenseClient);
    }

    private static async Task ExampleCreateCollection(ITypesenseClient typesenseClient)
    {
        var schema = new Schema(
            "Addresses",
            new List<Field>
            {
                new Field("id", FieldType.String, false),
                new Field("houseNumber", FieldType.Int32, false),
                new Field("accessAddress", FieldType.String, false, true),
                new Field("metadataNotes", FieldType.String, false, true, false),
            },
            "houseNumber");

        var createCollectionResponse = await typesenseClient.CreateCollection(schema);
        Console.WriteLine($"Created collection: {JsonSerializer.Serialize(createCollectionResponse)}");
    }

    private static async Task ExampleRetrieveCollection(ITypesenseClient typesenseClient)
    {
        var retrieveCollection = await typesenseClient.RetrieveCollection("Addresses");
        Console.WriteLine($"Retrieve collection: {JsonSerializer.Serialize(retrieveCollection)}");
    }

    private static async Task ExampleRetrieveCollections(ITypesenseClient typesenseClient)
    {
        var retrieveCollections = await typesenseClient.RetrieveCollections();
        Console.WriteLine($"Retrieve collections: {JsonSerializer.Serialize(retrieveCollections)}");
    }

    private static async Task ExampleInsertIntoCollection(ITypesenseClient typesenseClient)
    {
        var addressOne = new Address
        {
            HouseNumber = 1,
            AccessAddress = "Smedgade 25B"
        };

        // Example to show optional AccessAddress
        var addressTwo = new Address
        {
            HouseNumber = 2,
        };

        // Example to show non indexed field
        var addressThree = new Address
        {
            HouseNumber = 3,
            AccessAddress = "Singel 12",
            MetadataNotes = "This field is not indexed and will not use memory."
        };

        var houseOneResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressOne);
        Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseOneResponse)}");

        var houseTwoResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressTwo);
        Console.WriteLine($"Created document: {JsonSerializer.Serialize(addressTwo)}");

        var houseThreeResponse = await typesenseClient.CreateDocument<Address>("Addresses", addressThree);
        Console.WriteLine($"Created document: {JsonSerializer.Serialize(houseThreeResponse)}");
    }

    private static async Task ExampleExportAllDocumentsFromCollection(ITypesenseClient typesenseClient)
    {
        var exportResult = await typesenseClient.ExportDocuments<Address>("Addresses");
        Console.WriteLine($"Export result: {JsonSerializer.Serialize(exportResult)}");
    }

    private static async Task ExampleExportDocumentsWithExportParameters(ITypesenseClient typesenseClient)
    {
        var exportResultParameters = await typesenseClient.ExportDocuments<Address>(
            "Addresses",
            new ExportParameters
            {
                IncludeFields = "houseNumber"
            });

        Console.WriteLine($"Export result: {JsonSerializer.Serialize(exportResultParameters)}");
    }

    private static async Task ExampleUpsertDocument(ITypesenseClient typesenseClient)
    {
        // Example showcasing upserting a new address that is not already indexed.
        var notExistingAddress = new Address
        {
            HouseNumber = 25,
            AccessAddress = "Address"
        };

        var upsertNotExistingAddress = await typesenseClient.UpsertDocument<Address>(
            "Addresses", notExistingAddress);

        Console.WriteLine($"Upserted document: {JsonSerializer.Serialize(upsertNotExistingAddress)}");

        // Example showcasing upserting a new address that is already indexed.
        var existingAddress = new Address
        {
            Id = upsertNotExistingAddress.Id,
            HouseNumber = 25,
            AccessAddress = "Awesome new access address!"
        };

        var upsertExistingAddress = await typesenseClient.UpsertDocument<Address>(
            "Addresses", existingAddress);

        Console.WriteLine($"Upserted document: {JsonSerializer.Serialize(upsertExistingAddress)}");
    }

    private static async Task ExampleImportDocuments(ITypesenseClient typesenseClient)
    {
        var addresses = new List<Address>
            {
                new Address { AccessAddress = "Sulstreet 4", HouseNumber = 223 },
                new Address { AccessAddress = "Sulstreet 24", HouseNumber = 321 }
            };

        var importDocuments = await typesenseClient.ImportDocuments<Address>(
            "Addresses", addresses, 40, ImportType.Create);

        Console.WriteLine($"Import documents: {JsonSerializer.Serialize(importDocuments)}");
    }

    private static async Task ExampleUpdatingExistingDocument(ITypesenseClient typesenseClient)
    {
        var address = await typesenseClient.RetrieveDocument<Address>("Addresses", "4");

        address.HouseNumber = 1;

        var updateDocumentResult = await typesenseClient.UpdateDocument<Address>("Addresses", "4", address);
        Console.WriteLine($"Updated document: ${JsonSerializer.Serialize(updateDocumentResult)}");
    }

    private static async Task ExampleDoingSearch(ITypesenseClient typesenseClient)
    {
        var query = new SearchParameters("Sul", "accessAddress");
        var searchResult = await typesenseClient.Search<Address>("Addresses", query);
        Console.WriteLine($"Search result: {JsonSerializer.Serialize(searchResult)}");
    }

    private static async Task ExampleRetrieveDocumentOnId(ITypesenseClient typesenseClient)
    {
        var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
        Console.WriteLine($"Retrieved document: {JsonSerializer.Serialize(retrievedDocument)}");
    }

    private static async Task ExampleHandlingTypesenseApiException(ITypesenseClient typesenseClient)
    {
        try
        {
            var documentNotExist = await typesenseClient.RetrieveDocument<Address>("Addresses", "1120");
        }
        catch (TypesenseApiNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static async Task ExampleCreatingApiKeys(ITypesenseClient typesenseClient)
    {
        var keyOne = new Key(
            "Example key one",
            new[] { "*" },
            new[] { "*" })
        {
            Value = "Example-api-1-key-value",
            ExpiresAt = 1661344547
        };

        var keyTwo = new Key(
            "Example key two",
            new[] { "*" },
            new[] { "*" })
        {
            Value = "Example-api-2-key-value",
            ExpiresAt = 1661344547
        };

        var createKeyResultOne = await typesenseClient.CreateKey(keyOne);
        Console.WriteLine($"Created key: {JsonSerializer.Serialize(createKeyResultOne)}");

        var createKeyResultTwo = await typesenseClient.CreateKey(keyTwo);
        Console.WriteLine($"Created key: {JsonSerializer.Serialize(createKeyResultTwo)}");
    }

    private static async Task ExampleRetrieveApiKey(ITypesenseClient typesenseClient)
    {
        var retrievedKey = await typesenseClient.RetrieveKey(1);
        Console.WriteLine($"Retrieved key: {JsonSerializer.Serialize(retrievedKey)}");
    }

    private static async Task ExampleListApiKeys(ITypesenseClient typesenseClient)
    {
        var listKeys = await typesenseClient.ListKeys();
        Console.WriteLine($"List keys: {JsonSerializer.Serialize(listKeys)}");
    }

    private static void ExampleGenerateScopedApiKey(ITypesenseClient typesenseClient)
    {
        var scopedSearchKey = typesenseClient.GenerateScopedSearchKey(
            "my-awesome-security-key", "{\"filter_by\":\"accessible_to_user_ids:2\"}");

        Console.WriteLine($"Scoped Search Key: {scopedSearchKey}");
    }

    private static async Task ExampleUpsertSearchOverride(ITypesenseClient typesenseClient)
    {
        var searchOverride = new SearchOverride(new Rule("Sul", "exact"))
        {
            Includes = new List<Include> { new Include("2", 1) },
        };

        var upsertSearchOverrideResponse = await typesenseClient.UpsertSearchOverride(
            "Addresses",
            "addresses-override",
            searchOverride);

        Console.WriteLine($"Upsert search override: {JsonSerializer.Serialize(upsertSearchOverrideResponse)}");
    }

    private static async Task ExampleListAllSearchOverridesForCollection(ITypesenseClient typesenseClient)
    {
        var listSearchOverrides = await typesenseClient.ListSearchOverrides("Addresses");
        Console.WriteLine($"List search overrides: {JsonSerializer.Serialize(listSearchOverrides)}");
    }


    private static async Task ExampleRetrieveSearchOverrideInCollection(ITypesenseClient typesenseClient)
    {
        var retrieveSearchOverride = await typesenseClient.RetrieveSearchOverride(
            "Addresses", "addresses-override");
        Console.WriteLine($"retrieve search override: {JsonSerializer.Serialize(retrieveSearchOverride)}");
    }

    private static async Task ExampleUpsertCollectionAlias(ITypesenseClient typesenseClient)
    {
        var upsertCollectionAlias = await typesenseClient.UpsertCollectionAlias(
            "Address_Alias", new CollectionAlias("Addresses"));

        Console.WriteLine($"Upsert alias: {JsonSerializer.Serialize(upsertCollectionAlias)}");
    }

    private static async Task ExampleRetrieveCollectionAlias(ITypesenseClient typesenseClient)
    {
        var retrieveCollectionAlias = await typesenseClient.RetrieveCollectionAlias("Address_Alias");
        Console.WriteLine($"retrieve alias: {JsonSerializer.Serialize(retrieveCollectionAlias)}");
    }

    private static async Task ExampleListAllCollectionAliases(ITypesenseClient typesenseClient)
    {
        var listCollectionAliases = await typesenseClient.ListCollectionAliases();
        Console.WriteLine($"List alias: {JsonSerializer.Serialize(listCollectionAliases)}");
    }

    private static async Task ExampleUpsertSynonym(ITypesenseClient typesenseClient)
    {
        var upsertSynonym = await typesenseClient.UpsertSynonym(
            "Addresses", "Address_Synonym", new SynonymSchema(new List<string> { "Sultan", "Soltan", "Softan" }));

        Console.WriteLine($"Upsert synonym: {JsonSerializer.Serialize(upsertSynonym)}");
    }

    private static async Task ExampleRetrieveSynonymInCollection(ITypesenseClient typesenseClient)
    {
        var retrieveSynonym = await typesenseClient.RetrieveSynonym("Addresses", "Address_Synonym");
        Console.WriteLine($"Retrieve synonym: {JsonSerializer.Serialize(retrieveSynonym)}");
    }

    private static async Task ExampleListAllSynonymsInCollection(ITypesenseClient typesenseClient)
    {
        var listSynonyms = await typesenseClient.ListSynonyms("Addresses");
        Console.WriteLine($"List synonyms: {JsonSerializer.Serialize(listSynonyms)}");
    }

    private static async Task ExampleDeleteCollectionAlias(ITypesenseClient typesenseClient)
    {
        var deleteCollectionAlias = await typesenseClient.DeleteCollectionAlias("Address_Alias");
        Console.WriteLine($"Delete alias: {JsonSerializer.Serialize(deleteCollectionAlias)}");
    }

    private static async Task ExampleDeleteSynonym(ITypesenseClient typesenseClient)
    {
        var deleteSynonym = await typesenseClient.DeleteSynonym("Addresses", "Address_Synonym");
        Console.WriteLine($"Delete synonym: {JsonSerializer.Serialize(deleteSynonym)}");
    }

    private static async Task ExampleDeleteApiKey(ITypesenseClient typesenseClient, int id)
    {
        var deletedKey = await typesenseClient.DeleteKey(id);
        Console.WriteLine($"Deleted key: {JsonSerializer.Serialize(deletedKey)}");
    }

    private static async Task ExampleDeleteDocumentInCollection(ITypesenseClient typesenseClient)
    {
        var deleteResult = await typesenseClient.DeleteDocument<Address>("Addresses", "2");
        Console.WriteLine($"Deleted document {JsonSerializer.Serialize(deleteResult)}");
    }

    private static async Task ExampleDeleteDocumentsWithFilter(ITypesenseClient typesenseClient)
    {
        var deleteFilterResult = await typesenseClient.DeleteDocuments("Addresses", "houseNumber:>=3", 100);
        Console.WriteLine($"Deleted amount: {deleteFilterResult.NumberOfDeleted}");
    }

    private static async Task ExampleDeleteSearchOverride(ITypesenseClient typesenseClient)
    {
        var deletedSearchOverrideResult = await typesenseClient.DeleteSearchOverride(
            "Addresses", "addresses-override");

        Console.WriteLine($"Deleted override: {JsonSerializer.Serialize(deletedSearchOverrideResult)}");
    }

    private static async Task ExampleDeleteCollection(ITypesenseClient typesenseClient)
    {
        var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
        Console.WriteLine($"Deleted collection: {JsonSerializer.Serialize(deleteCollectionResult)}");
    }
}
