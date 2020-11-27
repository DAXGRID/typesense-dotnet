# Typesense-dotnet

Unofficial .net client for Typesense HTTP API.

## NOTE Currently under development

## Create setup

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
        new Field("id", "string", false),
        new Field("houseNumber", "int32", false),
        new Field("accessAddress", "string", false),
    },
    DefaultSortingField = "houseNumber"
};

await typesenseClient.CreateCollection(schema);
```

## Create document

``` c#

var house = new House
{
    Id = "1",
    HouseNumber = 2,
    AccessAddress = "Smedgade 25B"
};

await typesenseClient.CreateDocument("Addresses", house);
```

## Query document

``` c#
var query = new SearchParameters
{
    Text = "Smed",
    QueryBy = "accessAddress"
};

var searchResult = await typesenseClient.Search("Addresses", query);
```
