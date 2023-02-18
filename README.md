# Typesense-dotnet

.net client for [Typesense.](https://github.com/typesense/typesense)

You can get the NuGet package [here.](https://www.nuget.org/packages/Typesense/)

Feel free to make issues or create pull requests if you find any bugs or there are missing features.

## Setup

Setup in service collection so it can be dependency injected. The `AddTypesenseClient` can be found in the `Typesense.Setup` namespace. Remember to change the settings to match your Typesense service. Right now you can specify multiple nodes, but the implementation has not been completed yet, so if you want to use this for multiple nodes, then put a load balancer in front of your services and point the settings to your load balancer.

```c#
var provider = new ServiceCollection()
    .AddTypesenseClient(config =>
    {
        config.ApiKey = "mysecretapikey";
        config.Nodes = new List<Node>
        {
            new Node("localhost", "8108", "http")
        };
    }).BuildServiceProvider();
```

After that you can get it from the `provider` instance or dependency inject it.
```c#
var typesenseClient = provider.GetService<ITypesenseClient>();
```

## Create collection

When you create the collection, you can specify each field with `name`, `type` and if it should be a `facet`, an `optional` or an `indexed` field.

``` c#
var schema = new Schema(
    "Addresses",
    new List<Field>
    {
        new Field("id", FieldType.Int32, false),
        new Field("houseNumber", FieldType.Int32, false),
        new Field("accessAddress", FieldType.String, false, true),
        new Field("metadataNotes", FieldType.String, false, true, false),
    },
    "houseNumber");

var createCollectionResult = await typesenseClient.CreateCollection(schema);
```

The example uses `camelCase` by default for field names, but you override this on the document you want to insert. Below is an example using `snake_case`.

```C#
public class Address
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("house_number")]
    public int HouseNumber { get; set; }
    [JsonPropertyName("access_address")]
    public string AccessAddress { get; set; }
    [JsonPropertyName("metadata_notes")]
    public string MetadataNotes { get; set; }
}
```

## Update collection

Update existing collection.

```C#
var updateSchema = new UpdateSchema(new List<UpdateSchemaField>
{
    // Example deleting existing field.
    new UpdateSchemaField("metadataNotes", drop: true),
    // Example adding a new field.
    new UpdateSchemaField("city", FieldType.String, facet: false, optional: true)
});

var updateCollectionResponse = await typesenseClient.UpdateCollection("Addresses", updateSchema);
```

## Index document

```c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var createDocumentResult = await typesenseClient.CreateDocument<Address>("Addresses", address);
```

## Upsert document

```c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var upsertResult = await typesenseClient.UpsertDocument<Address>("Addresses", address);
```

## Search document in collection

```c#
var query = new SearchParameters("Smed", "accessAddress");
var searchResult = await typesenseClient.Search<Address>("Addresses", query);
```

## Search grouped

```c#
var query = new GroupedSearchParameters("Stark", "company_name", "country");
var response = await _client.SearchGrouped<Company>("companies", query);
```

## Multi search documents

Multi search goes from one query to four, if you need more than that please open an issue and I'll implement more.

Example of using a single query in multi-search.

```c#
var query = new MultiSearchParameters("companies", "Stark", "company_name");

var response = await _client.MultiSearch<Company>(queryOne);
```


Example of using a two queries in multi-search.

```c#
var queryOne = new MultiSearchParameters("companies", "Stark", "company_name");
var queryTwo = new MultiSearchParameters("employees", "Kenny", "person_name");

var (r1, r2) = await _client.MultiSearch<Company, Employee>(queryOne, queryTwo);
```

Example of using a three queries in multi-search (you get the pattern currently it goes up to four.).

```c#
var queryOne = new MultiSearchParameters("companies", "Stark", "company_name");
var queryTwo = new MultiSearchParameters("employees", "Kenny", "person_name");
var queryThree = new MultiSearchParameters("companies", "Awesome Corp.", "company_name");

var (r1, r2, r3) = await _client.MultiSearch<Company, Employee, Company>(queryOne, queryTwo, queryThree);
```

## Retrieve a document on id

```c#
var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
```

## Update document on id

```c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var updateDocumentResult = await typesenseClient.UpdateDocument<Address>("Addresses", "1", address);
```

## Delete document on id

```c#
var deleteResult = await typesenseClient.DeleteDocument<Address>("Addresses", "1");
```

## Delete documents using filter

```c#
var deleteResult = await typesenseClient.DeleteDocuments("Addresses", "houseNumber:>=3", 100);
```

## Drop a collection on name

```c#
var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
```

## Import documents

The default batch size is `40`.
The default ImportType is `Create`.
You can pick between three different import types `Create`, `Upsert`, `Update`.
The returned values are a list of `ImportResponse` that contains a `success code`, `error` and the failed `document` as a string representation.

```c#

var importDocumentResults = await typesenseClient.ImportDocuments<Address>("Addresses", addresses, 40, ImportType.Create);
```

## Export documents

```c#
var addresses = await typesenseClient.ExportDocuments<Address>("Addresses");
```

## Api keys


### Create key

`ExpiresAt` is optional.
`Value` is optional.

```c#
var apiKey = new Key(
    "Example key one",
    new[] { "*" },
    new[] { "*" });

var createdKey = await typesenseClient.CreateKey(apiKey);
```

### Retrieve key

```c#
var retrievedKey = await typesenseClient.RetrieveKey(0);
```

### List keys

```c#
var keys = await typesenseClient.ListKeys();
```

### Delete key

```c#
var deletedKey = await typesenseClient.DeleteKey(0);
```

### Generate Scoped Search key

```c#
var scopedSearchKey = typesenseClient.GenerateScopedSearchKey("MainOrParentAPIKey", "{\"filter_by\":\"accessible_to_user_ids:2\"}");
```

## Curation

While Typesense makes it really easy and intuitive to deliver great search results, sometimes you might want to promote certain documents over others. Or, you might want to exclude certain documents from a query's result set.

Using overrides, you can include or exclude specific documents for a given query.


### Upsert

```c#
var searchOverride = new SearchOverride(new List<Include> { new Include("2", 1) }, new Rule("Sul", "exact"));
var upsertSearchOverrideResponse = await typesenseClient.UpsertSearchOverride("Addresses", "addresses-override", searchOverride);
```

### List all overrides

```c#
var listSearchOverrides = await typesenseClient.ListSearchOverrides("Addresses");
```

### Retrieve overrides

```c#
var retrieveSearchOverride = await typesenseClient.RetrieveSearchOverride("Addresses", "addresses-override");
```

### Delete override

```c#
var deletedSearchOverrideResult = await typesenseClient.DeleteSearchOverride("Addresses", "addresses-override");
```

## Collection alias

An alias is a virtual collection name that points to a real collection. Read more [here.](https://typesense.org/docs/0.21.0/api/collection-alias.html)

### Upsert collection alias

```c#
var upsertCollectionAlias = await typesenseClient.UpsertCollectionAlias("Address_Alias", new CollectionAlias("Addresses"));
```

### List all collection aliases

```c#
var listCollectionAliases = await typesenseClient.ListCollectionAliases();
```

### Retrieve collection alias

```c#
var retrieveCollectionAlias = await typesenseClient.RetrieveCollectionAlias("Address_Alias");
```

### Delete collection alias

```c#
var deleteCollectionAlias = await typesenseClient.DeleteCollectionAlias("Addresses_Alias");
```

## Synonyms

The synonyms feature allows you to define search terms that should be considered equivalent. For eg: when you define a synonym for sneaker as shoe, searching for sneaker will now return all records with the word shoe in them, in addition to records with the word sneaker. Read more [here.](https://typesense.org/docs/0.21.0/api/synonyms.html#synonyms)

### Upsert synonym

```c#
var upsertSynonym = await typesenseClient.UpsertSynonym("Addresses", "Address_Synonym", new SynonymSchema(new List<string> { "Sultan", "Soltan", "Softan" }));
```

### Retrieve a synonym

```c#
var retrieveSynonym = await typesenseClient.RetrieveSynonym("Addresses", "Address_Synonym");
```

### List all synonyms

```c#
var listSynonyms = await typesenseClient.ListSynonyms("Addresses");
```

### Delete synonym

```c#
var deleteSynonym = await typesenseClient.DeleteSynonym("Addresses", "Address_Synonym");
```

## Metrics

Get current RAM, CPU, Disk & Network usage metrics.

```c#
var metrics = await typesenseClient.RetrieveMetrics();
```

## Stats

Get stats about API endpoints.

```c#
var stats = await typesenseClient.RetrieveStats();
```

### Typesense API Errors

Typesense API exceptions in the [Typesense-api-errors](https://typesense.org/docs/0.23.0/api/api-errors.html) spec.

| Type                                       | Description                                                                |
|:-------------------------------------------|:---------------------------------------------------------------------------|
| `TypesenseApiException`                    | Base exception type for Typesense api exceptions.                          |
| `TypesenseApiBadRequestException`          | Bad Request - The request could not be understood due to malformed syntax. |
| `TypesenseApiUnauthorizedException`        | Unauthorized - Your API key is wrong.                                      |
| `TypesenseApiNotFoundException`            | Not Found - The requested resource is not found.                           |
| `TypesenseApiConflictException`            | Conflict - When a resource already exists.                                 |
| `TypesenseApiUnprocessableEntityException` | Unprocessable Entity - Request is well-formed, but cannot be processed.    |
| `TypesenseApiServiceUnavailableException`  | Service Unavailable - We’re temporarily offline. Please try again later.   |

## Tests

Running all tests.

```sh
dotnet test
```

### Running only unit tests
```sh
dotnet test --filter Category=Unit
```

### Running integration tests

```sh
dotnet test --filter Category=Integration
```

To enable running integration tests you can run Typesense in a docker container using the command below.

```sh
docker run -p 8108:8108 -v/tmp/data:/data typesense/typesense:0.24.0 --data-dir /data --api-key=key
```
