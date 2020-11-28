# Typesense-dotnet

[Unofficial .net client for Typesense HTTP API.](https://www.nuget.org/packages/Typesense/)

## NOTE Currently under development

### Progress

- [x] Create collection
- [x] Create document
- [ ] Upsert document
- [ ] Update document
- [ ] Bulk documents
- [x] Search collection
- [ ] Delete document
- [x] Retrieve collection
- [ ] Export documents
- [ ] Import documents
- [ ] List all collections
- [ ] Drop a collection

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

await typesenseClient.CreateCollection(schema);
```

## Index document

``` c#

var address = new Address
{
    Id = 1,
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

await typesenseClient.CreateDocument("Addresses", address);
```

## Query document

``` c#
var query = new SearchParameters
{
    Text = "Smed",
    QueryBy = "accessAddress"
};

var searchResult = await typesenseClient.Search<House>("Addresses", query);
```
