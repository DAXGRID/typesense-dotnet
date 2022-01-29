using FluentAssertions;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Typesense.Tests;

public record Company()
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("company_name")]
    public string CompanyName { get; init; }
    [JsonPropertyName("num_employees")]
    public int NumEmployees { get; init; }
    [JsonPropertyName("country")]
    public string Country { get; init; }
}

[TestCaseOrderer("Typesense.Tests.PriorityOrderer", "Typesense.Tests")]
public class TypesenseClientTests : IClassFixture<TypesenseFixture>
{
    private readonly ITypesenseClient _client;

    public TypesenseClientTests(TypesenseFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact, TestPriority(0)]
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
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

    [Fact, TestPriority(4)]
    [Trait("Category", "Integration")]
    public async Task Index_document()
    {
        // We create collection for test cases.
        await CreateCompanyCollection();

        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Country = "USA",
        };

        var response = await _client.CreateDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(5)]
    [Trait("Category", "Integration")]
    public async Task Upsert_document_existing_document()
    {
        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Country = "USA",
        };

        var response = await _client.UpsertDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(6)]
    [Trait("Category", "Integration")]
    public async Task Upsert_document_new_document()
    {
        var company = new Company
        {
            Id = "999",
            CompanyName = "Awesome A/S",
            NumEmployees = 10,
            Country = "SWE",
        };

        var response = await _client.UpsertDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(7)]
    [Trait("Category", "Integration")]
    public async Task Import_documents_create()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse { Success = true },
            new ImportResponse { Success = true },
        };

        var companies = new List<Company>
        {
             new Company
             {
                 Id = "125",
                 CompanyName = "Future Technology",
                 NumEmployees = 1232,
                 Country = "UK",
             },
             new Company
             {
                 Id = "126",
                 CompanyName = "Random Corp.",
                 NumEmployees = 531,
                 Country = "AU",
             }
        };

        var response = await _client.ImportDocuments<Company>("companies", companies, 40, ImportType.Create);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(7)]
    [Trait("Category", "Integration")]
    public async Task Import_documents_update()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse { Success = true },
            new ImportResponse { Success = true },
        };

        var companies = new List<Company>
        {
             new Company
             {
                 Id = "125",
                 CompanyName = "Future Technology",
                 NumEmployees = 1233,
                 Country = "UK",
             },
             new Company
             {
                 Id = "126",
                 CompanyName = "Random Corp.",
                 NumEmployees = 532,
                 Country = "AU",
             }
        };

        var response = await _client.ImportDocuments<Company>("companies", companies, 40, ImportType.Update);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(7)]
    [Trait("Category", "Integration")]
    public async Task Import_documents_upsert()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse { Success = true },
            new ImportResponse { Success = true },
        };

        var companies = new List<Company>
        {
             new Company
             {
                 Id = "125",
                 CompanyName = "Future Technology",
                 NumEmployees = 1232,
                 Country = "UK",
             },
             new Company
             {
                 Id = "126",
                 CompanyName = "Random Corp.",
                 NumEmployees = 531,
                 Country = "AU",
             }
        };

        var response = await _client.ImportDocuments<Company>(
            "companies", companies, 40, ImportType.Upsert);

        response.Should().BeEquivalentTo(expected);
    }

    private async Task CreateCompanyCollection()
    {
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

        _ = await _client.CreateCollection(schema);
    }
}
