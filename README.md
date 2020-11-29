# Typesense-dotnet

[Unofficial .net client for Typesense HTTP API.](https://www.nuget.org/packages/Typesense/)

## NOTE Currently under development

### Progress

- [x] Create collection
- [x] Create document
- [x] Upsert document
- [x] Search collection
- [x] Retrieve document
- [x] Update document
- [x] Delete document
- [x] Retrieve collection
- [ ] Export documents
- [x] Import documents
- [ ] List all collections
- [x] Drop a collection

## Setup

Setup in service collection. The `AddTypesenseClient` can be found in the `Typesense.Setup` namespace.

``` c#
var provider = new ServiceCollection()
    .AddTypesenseClient(config =>
    {
        config.ApiKey = "Hu52dwsas2AdxdE";
        config.Nodes = new List<Node>
        {
            new Node
            {
                Host = "localhost",
                Port = "8108", Protocol = "http"
            }
        };
    }).BuildServiceProvider();
```

## Create collection

``` c#
var schema = new Schema
{
    Name = "Addresses",
    Fields = new List<Field>
    {
        new Field("id", "int32", false),
        new Field("houseNumber", "int32", false),
        new Field("accessAddress", "string", false),
    },
    DefaultSortingField = "id"
};

var createCollectionResult = await typesenseClient.CreateCollection(schema);
```

## Index document

``` c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var createDocumentResult = await typesenseClient.CreateDocument<Address>("Addresses", address);
```

## Upsert document

``` c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var upsertResult = await typesenseClient.UpsertDocument<Address>("Addresses", address);
```

## Search document in collection

``` c#
var query = new SearchParameters
{
    Text = "Smed",
    QueryBy = "accessAddress"
};

var searchResult = await typesenseClient.Search<Address>("Addresses", query);
```

## Retrieve a document on id

``` c#
var retrievedDocument = await typesenseClient.RetrieveDocument<Address>("Addresses", "1");
```

## Update document on id

``` c#
var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

var updateDocumentResult = await typesenseClient.UpdateDocument<Address>("Addresses", "1", address);
```

## Delete document on id

``` c#
var deleteResult = await typesenseClient.DeleteDocument<Address>("Addresses", "1");
```

## Delete documents using filter

``` c#
var deleteResult = await typesenseClient.DeleteDocuments("Addresses", "houseNumber:>=3", 100);
```

## Drop a collection on name

``` c#
var deleteCollectionResult = await typesenseClient.DeleteCollection("Addresses");
```

## Import documents 

The default batch size is `40`.
The default ImportType is `Create`.
You can pick between three different import types `Create`, `Upsert`, `Update`.
The returned values are a list of `ImportResponse` that contains a `success code`, `error` and the failed `document` as a string representation.

``` c#
var importDocumentResults = await typesenseClient.ImportDocuments<Address>("Addresses", addresses, 40, ImportType.Create);
```
