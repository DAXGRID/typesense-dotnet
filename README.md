# Typesense-dotnet

.net client for [Typesense.](https://github.com/typesense/typesense)

You can get the NuGet package [here.](https://www.nuget.org/packages/Typesense/)

Feel free to make issues or create pull requests if you find any bugs or there are missing features.

## Setup

Setup in service collection so it can be dependency injected. The `AddTypesenseClient` can be found in the `Typesense.Setup` namespace. Remember to change the settings to match your Typesense service. Right now you can specify multiple nodes, but the implementation has not been completed yet, so if you want to use this for multiple nodes, then put a load balancer in front of your services and point the settings to your load balancer.

``` c#
var provider = new ServiceCollection()
    .AddTypesenseClient(config =>
    {
        config.ApiKey = "mysecretapikey";
        config.Nodes = new List<Node>
        {
            new Node
            {
                Host = "localhost",
                Port = "8108",
                Protocol = "http"
            }
        };
    }).BuildServiceProvider();
```

After that you can get it from the `provider` instance or dependency inject it.
``` c#
var typesenseClient = provider.GetService<ITypesenseClient>();
```

## Create collection

When you create the collection, you can specify each field with `name`, `type` and if it should be a `facet` or be an `optional` field.

``` c#
var schema = new Schema
{
    Name = "Addresses",
    Fields = new List<Field>
    {
        new Field("id", "int32", false),
        new Field("houseNumber", "int32", false),
        new Field("accessAddress", "string", false, true),
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

## Export documents

``` c#
var addresses = await typesenseClient.ExportDocuments<Address>("Addresses");
```
