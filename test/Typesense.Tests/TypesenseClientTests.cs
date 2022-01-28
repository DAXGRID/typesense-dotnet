using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Typesense.Tests;

[TestCaseOrderer("Typesense.Tests.PriorityOrderer", "Typesense.Tests")]
public class TypesenseClientTests : IClassFixture<TypesenseFixture>
{
    private readonly ITypesenseClient _client;

    public TypesenseClientTests(TypesenseFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact, TestPriority(0)]
    public async Task Create_schema()
    {
        var expected = new CollectionResponse
        {
            Name = "companies",
            NumberOfDocuments = 0,
            Fields = new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, false),
                new Field("country", FieldType.String, true),
            },
            DefaultSortingField = "num_employees"
        };

        var schema = new Schema
        {
            Name = "companies",
            Fields = new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, false),
                new Field("country", FieldType.String, true),
            },
            DefaultSortingField = "num_employees"
        };

        var response = await _client.CreateCollection(schema);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(1)]
    public async Task Retrieve_collection()
    {
        var expected = new CollectionResponse
        {
            Name = "companies",
            NumberOfDocuments = 0,
            Fields = new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, false),
                new Field("country", FieldType.String, true),
            },
            DefaultSortingField = "num_employees"
        };

        var response = await _client.RetrieveCollection("companies");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(2)]
    public async Task Retrieve_collections()
    {
        var expected = new List<CollectionResponse>
        {
            new CollectionResponse
            {
                Name = "companies",
                NumberOfDocuments = 0,
                Fields = new List<Field>
                {
                    new Field("company_name", FieldType.String, false),
                    new Field("num_employees", FieldType.Int32, false),
                    new Field("country", FieldType.String, true),
                },
                DefaultSortingField = "num_employees"
            }
        };

        var response = await _client.RetrieveCollections();
        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(3)]
    public async Task Delete_collection()
    {
        var expected = new CollectionResponse
        {
            Name = "companies",
            NumberOfDocuments = 0,
            Fields = new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, false),
                new Field("country", FieldType.String, true),
            },
            DefaultSortingField = "num_employees"
        };

        var result = await _client.DeleteCollection("companies");

        result.Should().BeEquivalentTo(expected);
    }
}
