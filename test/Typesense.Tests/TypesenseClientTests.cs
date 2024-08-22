using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Typesense.Tests;

// The `AddressVectorSearch` type is created to test vector search functionality in Typesense.
public record AddressVectorSearch
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("vec")]
    public float[] Vec { get; init; }
}

// The `CourseSemanticSearch` type is created to test semantic search functionality in Typesense.
public record CourseSemanticSearch
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; }
}

public record Location
{
    [JsonPropertyName("country")]
    public string Country { get; init; }
    [JsonPropertyName("city")]
    public string City { get; init; }
}

public record Company()
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("company_name")]
    public string CompanyName { get; init; }
    [JsonPropertyName("num_employees")]
    public int NumEmployees { get; init; }
    [JsonPropertyName("location")]
    public Location Location { get; init; }
    [JsonPropertyName("aliases")]
    public IReadOnlyCollection<string> Aliases { get; init; }
}

[Trait("Category", "Integration")]
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
        var expected = new CollectionResponse(
            "companies",
            0,
            new List<Field>
            {
                new Field(
                    "company_name",
                    type: FieldType.String,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "location",
                    type: FieldType.Object,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
            },
            "num_employees",
            new List<string>(),
            new List<string>(),
            true);

        var schema = new Schema(
            "companies",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("location", FieldType.Object, true),
            },
            "num_employees")
        {
            EnableNestedFields = true
        };

        var response = await _client.CreateCollection(schema);

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(0)]
    public async Task Create_schema_symbols_to_index_and_token_seperator()
    {
        var expected = new CollectionResponse(
            "companies_with_symbols_and_token",
            0,
            new List<Field>
            {
                new Field(
                    name: "company_name",
                    type: FieldType.String,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "location",
                    type: FieldType.Object,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
            },
            "num_employees",
            new List<string> { "-" },
            new List<string> { "+" },
            true);

        var schema = new Schema(
            "companies_with_symbols_and_token",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("location", FieldType.Object, true),
            },
            "num_employees")
        {
            TokenSeparators = new List<string> { "-" },
            SymbolsToIndex = new List<string> { "+" },
            EnableNestedFields = true,
        };

        var response = await _client.CreateCollection(schema);

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);

        // Cleanup
        await _client.DeleteCollection(schema.Name);
    }

    // We test wildcard field individually,
    // because it is a bit different than other types of fields.
    [Fact, TestPriority(0)]
    public async Task Create_schema_with_wildcard_field()
    {
        var expected = new CollectionResponse(
            "wildcard-collection",
            0,
            new List<Field>
            {
                new Field(
                    name: ".*",
                    type: FieldType.String,
                    facet: false,
                    optional: true,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
            },
            "",
            new List<string>(),
            new List<string>(),
            false);

        var schema = new Schema(
            "wildcard-collection",
            new List<Field>
            {
                new Field(".*", FieldType.String, false),
            });

        var response = await _client.CreateCollection(schema);

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);

        // Cleanup
        await _client.DeleteCollection("wildcard-collection");
    }

    // We test wildcard field individually,
    // because it is a bit different than other types of fields.
    [Fact, TestPriority(0)]
    public async Task Create_schema_with_locale_on_field()
    {
        const string collectionName = "collection-with-locale";

        var expected = new CollectionResponse(
            collectionName,
            0,
            new List<Field>
            {
                new Field(
                    name: "name",
                    type: FieldType.String,
                    facet: false,
                    optional: true,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "zh")
                {
                    Stem = false,
                    Store = true
                },
            },
            "",
            new List<string>(),
            new List<string>(),
            false);

        var schema = new Schema(
            collectionName,
            new List<Field>
            {
                new Field(
                    "name",
                    FieldType.String,
                    optional: true,
                    locale: "zh"),
            });

        var response = await _client.CreateCollection(schema);

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);

        // Cleanup
        await _client.DeleteCollection(collectionName);
    }

    [Fact, TestPriority(1)]
    public async Task Retrieve_collection()
    {
        var expected = new CollectionResponse(
            "companies",
            0,
            new List<Field>
            {
                new Field(
                    name: "company_name",
                    type: FieldType.String,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    "location",
                    type: FieldType.Object,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
            },
            "num_employees",
            new List<string>(),
            new List<string>(),
            true);

        var response = await _client.RetrieveCollection("companies");

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(2)]
    public async Task Retrieve_collections()
    {
        var expected = new List<CollectionResponse>
        {
            new CollectionResponse(
                "companies",
                0,
                new List<Field>
                {
                    new Field(
                        name: "company_name",
                        type: FieldType.String,
                        facet: false,
                        optional: false,
                        index: true,
                        sort: false,
                        infix: false,
                        locale: "")
                    {
                        Stem = false,
                        Store = true
                    },
                    new Field(
                        name: "num_employees",
                        type: FieldType.Int32,
                        facet: true,
                        optional: false,
                        index: true,
                        sort: true,
                        infix: false,
                        locale: "")
                    {
                        Stem = false,
                        Store = true
                    },
                    new Field(
                        name: "location",
                        type: FieldType.Object,
                        facet: true,
                        optional: false,
                        index: true,
                        sort: false,
                        infix: false,
                        locale: "")
                    {
                        Stem = false,
                        Store = true
                    }
                },
                "num_employees",
                new List<string>(),
                new List<string>(),
                true)
        };

        var response = await _client.RetrieveCollections();

        response.Should().HaveCount(1);
        // CreatedAt cannot be deterministic
        response[0].CreatedAt.Should().NotBe(default);
        expected[0] = expected[0] with { CreatedAt = response[0].CreatedAt };

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(2)]
    public async Task Update_collection()
    {
        var updateSchema = new UpdateSchema(
            new List<UpdateSchemaField>
            {
                new UpdateSchemaField("company_name", true),
                new UpdateSchemaField("non_profit", FieldType.Bool)
            }
        );

        var result = await _client.UpdateCollection("companies", updateSchema);

        result.Fields.Should().Satisfy(
            e => e.Name == "company_name" && e.Drop == true,
            e => e.Name == "non_profit" && e.Type == FieldType.Bool
        );
    }

    [Fact, TestPriority(3)]
    public async Task Delete_collection()
    {
        var expected = new CollectionResponse(
            "companies",
            0,
            new List<Field>
            {
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "location",
                    type: FieldType.Object,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
                new Field(
                    name: "non_profit",
                    type: FieldType.Bool,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false,
                    locale: "")
                {
                    Stem = false,
                    Store = true
                },
            },
            "num_employees",
            new List<string>(),
            new List<string>(),
            true);

        var response = await _client.DeleteCollection("companies");

        // CreatedAt cannot be deterministic
        response.CreatedAt.Should().NotBe(default);
        expected = expected with { CreatedAt = response.CreatedAt };

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(4)]
    public async Task Index_document()
    {
        // We create collection for test cases.
        await CreateCompanyCollection();

        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var response = await _client.CreateDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(5)]
    public async Task Upsert_document_existing_document()
    {
        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var response = await _client.UpsertDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(6)]
    public async Task Upsert_document_new_document()
    {
        var company = new Company
        {
            Id = "999",
            CompanyName = "Awesome A/S",
            NumEmployees = 10,
            Location = new Location
            {
                City = "Stockholm",
                Country = "SWE"
            },
        };

        var response = await _client.UpsertDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(7)]
    public async Task Import_documents_create()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse(true),
            new ImportResponse(true),
        };

        var companies = new List<Company>
        {
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                NumEmployees = 1232,
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                NumEmployees = 531,
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            }
        };

        var response = await _client.ImportDocuments<Company>(
            "companies", companies, 40, ImportType.Create);

        response.Should().BeEquivalentTo(expected);

        foreach (var documentId in companies.Select(c => c.Id))
        {
            await _client.DeleteDocument<Company>("companies", documentId);
        }
        var companyLines = JsonLines(companies).ToList();
        response = await _client.ImportDocuments("companies", companyLines, 40, ImportType.Create);
        response.Should().BeEquivalentTo(expected);

        foreach (var documentId in companies.Select(c => c.Id))
        {
            await _client.DeleteDocument<Company>("companies", documentId);
        }
        response = await _client.ImportDocuments("companies", string.Join('\n', companyLines), 40, ImportType.Create);
        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(7)]
    public async Task Import_documents_update()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse(true),
            new ImportResponse(true),
        };

        var companies = new List<Company>
        {
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                NumEmployees = 1233,
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                NumEmployees = 532,
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            }
        };

        var response = await _client.ImportDocuments<Company>("companies", companies, 40, ImportType.Update);

        response.Should().BeEquivalentTo(expected);

        var companyLines = JsonLines(companies).ToList();
        response = await _client.ImportDocuments("companies", companyLines, 40, ImportType.Update);
        response.Should().BeEquivalentTo(expected);

        response = await _client.ImportDocuments("companies", string.Join('\n', companyLines), 40, ImportType.Update);
        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(7)]
    public async Task Import_documents_upsert()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse(true),
            new ImportResponse(true),
        };

        var companies = new List<Company>
        {
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                NumEmployees = 1232,
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                NumEmployees = 531,
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            }
        };

        var response = await _client.ImportDocuments<Company>(
            "companies", companies, 40, ImportType.Upsert);

        response.Should().BeEquivalentTo(expected);

        var companyLines = JsonLines(companies).ToList();
        response = await _client.ImportDocuments("companies", companyLines, 40, ImportType.Upsert);
        response.Should().BeEquivalentTo(expected);

        response = await _client.ImportDocuments("companies", string.Join('\n', companyLines), 40, ImportType.Upsert);
        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(7)]
    public async Task Import_documents_emplace()
    {
        var expected = new List<ImportResponse>
        {
            new ImportResponse(true),
            new ImportResponse(true),
        };

        var companies = new List<Company>
        {
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                NumEmployees = 1232,
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                NumEmployees = 531,
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            }
        };

        var response = await _client.ImportDocuments<Company>(
            "companies", companies, 40, ImportType.Emplace);

        response.Should().BeEquivalentTo(expected);

        var companyLines = JsonLines(companies).ToList();
        response = await _client.ImportDocuments("companies", companyLines, 40, ImportType.Emplace);
        response.Should().BeEquivalentTo(expected);

        response = await _client.ImportDocuments("companies", string.Join('\n', companyLines), 40, ImportType.Emplace);
        response.Should().BeEquivalentTo(expected);
    }
    private static readonly JsonSerializerOptions JsonOptionsCamelCaseIgnoreWritingNull = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static IEnumerable<string> JsonLines<T>(IEnumerable<T> documents)
        => documents.Select(document => JsonSerializer.Serialize(document, JsonOptionsCamelCaseIgnoreWritingNull));

    [Fact, TestPriority(8)]
    public async Task Export_documents()
    {
        var expected = new List<Company>
        {
            new Company
            {
                Id = "124",
                CompanyName = "Stark Industries",
                NumEmployees = 5215,
                Location = new Location
                {
                    City = "Phoenix",
                    Country = "USA"
                },
            },
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                NumEmployees = 1232,
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                NumEmployees = 531,
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "999",
                CompanyName = "Awesome A/S",
                NumEmployees = 10,
                Location = new Location
                {
                    City = "Stockholm",
                    Country = "SWE"
                },
            }
        };

        var response = await _client.ExportDocuments<Company>("companies");

        response.OrderBy(x => x.Id).Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(8)]
    public async Task Export_documents_filter_by_query()
    {
        var expected = new List<Company>
        {
            new Company
            {
                Id = "124",
                CompanyName = "Stark Industries",
                NumEmployees = 5215,
                Location = new Location
                {
                    City = "Phoenix",
                    Country = "USA"
                },
            }
        };

        var response = await _client.ExportDocuments<Company>("companies", new ExportParameters { FilterBy = "id: [124]" });

        response.OrderBy(x => x.Id).Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(8)]
    public async Task Export_documents_include_fields()
    {
        var expected = new List<Company>
        {
            new Company
            {
                Id = "124",
                CompanyName = "Stark Industries"
            },
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology"
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp."
            },
            new Company
            {
                Id = "999",
                CompanyName = "Awesome A/S"
            }
        };

        var response = await _client.ExportDocuments<Company>("companies", new ExportParameters { IncludeFields = "id,company_name" });

        response.OrderBy(x => x.Id).Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(8)]
    public async Task Export_documents_exclude_fields()
    {
        var expected = new List<Company>
        {
            new Company
            {
                Id = "124",
                CompanyName = "Stark Industries",
                Location = new Location
                {
                    City = "Phoenix",
                    Country = "USA"
                },
            },
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                Location = new Location
                {
                    City = "Aarhus",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                Location = new Location
                {
                    City = "Copenhagen",
                    Country = "DK"
                },
            },
            new Company
            {
                Id = "999",
                CompanyName = "Awesome A/S",
                Location = new Location
                {
                    City = "Stockholm",
                    Country = "SWE"
                },
            }
        };

        var response = await _client.ExportDocuments<Company>("companies", new ExportParameters { ExcludeFields = "num_employees" });

        response.OrderBy(x => x.Id).Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(9)]
    public async Task Retrieve_document()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var response = await _client.RetrieveDocument<Company>("companies", "124");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(10)]
    public async Task Update_document()
    {
        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var response = await _client.UpdateDocument("companies", "124", company);

        response.Should().BeEquivalentTo(response);
    }

    [Fact, TestPriority(11)]
    public async Task Search_query_by_just_text()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new SearchParameters("Stark", "company_name");

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_query_by_just_text_with_full_search_field()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new SearchParameters("Stark", "company_name")
        {
            HighlightFullFields = "company_name"
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
            // It's important that value is returned in the highlights, it is the main
            // reason to use 'HighlightFullFields'.
            response.Hits.First().Highlights.First().Value.Should().NotBeNullOrEmpty();
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_query_by_just_text_with_split_join_tokens_set()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new SearchParameters("Stark", "company_name")
        {
            SplitJoinTokens = SplitJoinTokenOption.Fallback
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_query_by_two_fields()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new SearchParameters("Stark", "company_name,location.country");
        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_facet_by_country()
    {
        var expected = new FacetCount("location.country", new List<FacetCountHit>
        {
            new FacetCountHit("DK", 2, "DK"),
            new FacetCountHit("SWE", 1, "SWE"),
            new FacetCountHit("USA", 1, "USA"),
        }, new FacetStats(0, 0, 0, 0, 3));

        var query = new SearchParameters("", "company_name")
        {
            FacetBy = "location.country"
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.FacetCounts.Should().HaveCount(1);
            response.FacetCounts.First().Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_facet_by_num_employees()
    {
        var expected = new FacetCount("num_employees", new List<FacetCountHit>
        {
            new FacetCountHit( "10", 1, "10"),
            new FacetCountHit( "531", 1, "531"),
            new FacetCountHit( "1232", 1, "1232"),
            new FacetCountHit( "6000", 1, "6000"),
        }, new FacetStats(1943.25F, 6000F, 10F, 7773F, 4));

        var query = new SearchParameters("", "company_name")
        {
            FacetBy = "num_employees"
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.FacetCounts.Should().HaveCount(1);
            response.FacetCounts.First().Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_facet_by_country_with_query()
    {
        var expected = new FacetCount("location.country", new List<FacetCountHit>
        {
            new FacetCountHit("USA", 1, "USA"),
        }, new FacetStats(0, 0, 0, 0, 1));

        var query = new SearchParameters("Stark", "company_name")
        {
            FacetBy = "location.country"
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.FacetCounts.Should().HaveCount(1);
            response.FacetCounts.First().Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Search_grouped_by_country()
    {
        var query = new GroupedSearchParameters("Stark", "company_name", "location.country");
        var response = await _client.SearchGrouped<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.FoundDocs.Value.Should().Be(1);
            response.GroupedHits.Should().NotBeEmpty();
            var firstHit = response.GroupedHits.First();
            firstHit.GroupKey.Should().BeEquivalentTo("USA");
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_single_type_multiple_multi_search_parameters()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var responses = await _client.MultiSearch<Company>(
            new List<MultiSearchParameters>
            {
                // The same queries are executed multiple times
                // because we do not care about the response,
                // but that we can do many multi searches at once.
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("companies", "Stark", "company_name")
            });

        using (var scope = new AssertionScope())
        {
            responses.Count().Should().Be(6);
            foreach (var response in responses)
            {
                response.Found.Should().Be(1);
                response.Hits.First().Document.Should().BeEquivalentTo(expected);
            }
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_single_type_multiple_multi_search_parameters_one_failed()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var responses = await _client.MultiSearch<Company>(
            new List<MultiSearchParameters>
            {
                // The same queries are executed multiple times
                // because we do not care about the response,
                // but that we can do many multi searches at once.
                new MultiSearchParameters("companies", "Stark", "company_name"),
                new MultiSearchParameters("this_collection_does_not_exist", "Stark", "company_name"),
            });

        using (var scope = new AssertionScope())
        {
            responses.Count().Should().Be(2);

            // Validate that first has no errors.
            responses[0].Found.Should().Be(1);
            responses[0].Hits.First().Document.Should().BeEquivalentTo(expected);
            responses[0].ErrorMessage.Should().BeNull();
            responses[0].ErrorCode.Should().BeNull();

            // Validate that second has errors.
            responses[1].Found.Should().BeNull();
            responses[1].Hits.Should().BeNull();
            responses[1].ErrorMessage.Should().Be("Not found.");
            responses[1].ErrorCode.Should().Be(404);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_single()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new MultiSearchParameters("companies", "Stark", "company_name");
        var response = await _client.MultiSearch<Company>(query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_two()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new MultiSearchParameters("companies", "Stark", "company_name");

        // We just use the same for queries for simplicity.
        // We mainly care about multiple queries being returned with the correct types of <T>.
        var (r1, r2) = await _client.MultiSearch<Company, Company>(query, query);

        using (var scope = new AssertionScope())
        {
            r1.Found.Should().Be(1);
            r1.Hits.First().Document.Should().BeEquivalentTo(expected);

            r2.Found.Should().Be(1);
            r2.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_three()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new MultiSearchParameters("companies", "Stark", "company_name");

        // We just use the same for queries for simplicity.
        // We mainly care about multiple queries being returned with the correct types of <T>.
        var (r1, r2, r3) = await _client.MultiSearch<Company, Company, Company>(query, query, query);

        using (var scope = new AssertionScope())
        {
            r1.Found.Should().Be(1);
            r1.Hits.First().Document.Should().BeEquivalentTo(expected);

            r2.Found.Should().Be(1);
            r2.Hits.First().Document.Should().BeEquivalentTo(expected);

            r3.Found.Should().Be(1);
            r3.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(11)]
    public async Task Multi_search_query_four()
    {
        var expected = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var query = new MultiSearchParameters("companies", "Stark", "company_name");

        // We just use the same for queries for simplicity.
        // We mainly care about multiple queries being returned with the correct types of <T>.
        var (r1, r2, r3, r4) = await _client.MultiSearch<Company, Company, Company, Company>(query, query, query, query);

        using (var scope = new AssertionScope())
        {
            r1.Found.Should().Be(1);
            r1.Hits.First().Document.Should().BeEquivalentTo(expected);

            r2.Found.Should().Be(1);
            r2.Hits.First().Document.Should().BeEquivalentTo(expected);

            r3.Found.Should().Be(1);
            r3.Hits.First().Document.Should().BeEquivalentTo(expected);

            r4.Found.Should().Be(1);
            r4.Hits.First().Document.Should().BeEquivalentTo(expected);
        }
    }

    [Fact, TestPriority(12)]
    public async Task Delete_document_by_id()
    {
        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 6000,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
        };

        var response = await _client.DeleteDocument<Company>("companies", "124");

        response.Should().BeEquivalentTo(response);
    }

    [Fact, TestPriority(13)]
    public async Task Delete_document_by_query()
    {
        var expected = new FilterDeleteResponse(2);
        var response = await _client.DeleteDocuments("companies", "num_employees:>100");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(14)]
    public async Task Create_api_key()
    {
        var expected = new Key(
            "Example key one",
            new[] { "*" },
            new[] { "*" })
        {
            Value = "Example-api-1-key-value",
            ExpiresAt = 1661344547
        };

        var response = await _client.CreateKey(expected);

        using (var scope = new AssertionScope())
        {
            response.Description.Should().BeEquivalentTo(expected.Description);
            response.Actions.Should().BeEquivalentTo(expected.Actions);
            response.Collections.Should().BeEquivalentTo(expected.Collections);
            response.Value.Should().BeEquivalentTo(expected.Value);
            response.ExpiresAt.Should().Be(expected.ExpiresAt);
        }
    }

    [Fact, TestPriority(15)]
    public async Task Retrieve_api_key()
    {
        var apiKeys = await _client.ListKeys();
        var apiKey = apiKeys.Keys.First();

        var response = await _client.RetrieveKey(apiKey.Id);

        response.Should().BeEquivalentTo(apiKey);
    }

    [Fact, TestPriority(16)]
    public async Task List_keys()
    {
        var expected = new Key(
            "Example key one",
            new[] { "*" },
            new[] { "*" })
        {
            Value = "Example-api-1-key-value",
            ExpiresAt = 1661344547
        };

        var response = await _client.ListKeys();

        response.Keys.Should()
            .HaveCount(1).And
            .SatisfyRespectively(
                first =>
                {
                    first.Description.Should().BeEquivalentTo(expected.Description);
                    first.Actions.Should().BeEquivalentTo(expected.Actions);
                    first.Collections.Should().BeEquivalentTo(expected.Collections);
                    first.Value.Should().NotBeEmpty();
                    first.ExpiresAt.Should().Be(expected.ExpiresAt);
                });
    }

    [Fact, TestPriority(17)]
    public async Task Delete_api_key()
    {
        var apiKeys = await _client.ListKeys();
        var apiKey = apiKeys.Keys.First(x => x.Description.Equals("Example key one"));
        var expected = new DeleteKeyResponse
        {
            Id = apiKey.Id
        };

        var response = await _client.DeleteKey(apiKey.Id);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(18)]
    public async Task Upsert_search_override()
    {
        var searchOverride = new SearchOverride(
            new Rule("apple", "exact"))
        {
            Includes = new List<Include>
                {
                    new Include("422", 1),
                    new Include("54", 2)
                },
        };

        var response = await _client.UpsertSearchOverride(
            "companies", "customize-apple", searchOverride);
    }

    [Fact, TestPriority(19)]
    public async Task Retrive_search_override()
    {
        var searchOverrides = await _client.ListSearchOverrides("companies");

        var expected = searchOverrides.SearchOverrides.First();

        var response = await _client.RetrieveSearchOverride("companies", expected.Id);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(20)]
    public async Task List_search_overrides()
    {
        var expected = new SearchOverride(
            new Rule("apple", "exact"))
        {
            Includes = new List<Include>
                {
                    new Include("422", 1),
                    new Include("54", 2)
                },
        };

        var response = await _client.ListSearchOverrides("companies");

        response.SearchOverrides.Should()
            .HaveCount(1).And
            .SatisfyRespectively(
                first =>
                {
                    first.Id.Should().Be("customize-apple");
                    first.Includes.Should().BeEquivalentTo(expected.Includes);
                    first.Rule.Should().BeEquivalentTo(expected.Rule);
                });
    }

    [Fact, TestPriority(21)]
    public async Task Delete_search_override()
    {
        var expected = new DeleteSearchOverrideResponse("customize-apple");

        var response = await _client.DeleteSearchOverride("companies", "customize-apple");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(22)]
    [Trait("Category", "Integration")]
    public async Task Upsert_collection_alias()
    {
        var expected = new CollectionAliasResponse("companies", "my-companies-alias");

        var response = await _client.UpsertCollectionAlias(
            "my-companies-alias", new CollectionAlias("companies"));

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(23)]
    public async Task List_collection_aliases()
    {
        var expected = new ListCollectionAliasesResponse(new List<CollectionAliasResponse>
        {
            new CollectionAliasResponse("companies", "my-companies-alias")
        });

        var response = await _client.ListCollectionAliases();

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(24)]
    public async Task Retrieve_collection_alias()
    {
        var expected = new CollectionAliasResponse("companies", "my-companies-alias");

        var response = await _client.RetrieveCollectionAlias("my-companies-alias");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(25)]
    public async Task Delete_collection_alias()
    {
        var expected = new CollectionAliasResponse("companies", "my-companies-alias");

        var response = await _client.DeleteCollectionAlias("my-companies-alias");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(26)]
    public async Task Upsert_synonym()
    {
        var expected = new SynonymSchemaResponse(
            "apple-synonyms",
            new List<string> { "appl", "aple", "apple" },
            "apple",
            new List<string> { "+" });

        var schema = new SynonymSchema(new List<string> { "appl", "aple", "apple" })
        {
            Root = "apple",
            SymbolsToIndex = new List<string> { "+" }
        };

        var response = await _client.UpsertSynonym("companies", "apple-synonyms", schema);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(27)]
    public async Task Retrieve_synonym()
    {
        var expected = new SynonymSchemaResponse(
            "apple-synonyms",
            new List<string> { "appl", "aple", "apple" },
            "apple",
            new List<string> { "+" });

        var response = await _client.RetrieveSynonym("companies", "apple-synonyms");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(28)]
    public async Task List_synonyms()
    {
        var expected = new ListSynonymsResponse(
            new List<SynonymSchemaResponse>
            {
                new SynonymSchemaResponse(
                    "apple-synonyms",
                    new List<string> { "appl", "aple", "apple" },
                    "apple",
                    new List<string> { "+" })
            });

        var response = await _client.ListSynonyms("companies");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(29)]
    public async Task Delete_synonym()
    {
        var expected = new DeleteSynonymResponse("apple-synonyms");
        var response = await _client.DeleteSynonym("companies", "apple-synonyms");

        response.Should().Be(expected);
    }

    [Fact, TestPriority(30)]
    public async Task Can_retrieve_metrics()
    {
        var response = await _client.RetrieveMetrics();

        using (var scope = new AssertionScope())
        {
            response.Should().NotBeNull();
            response.SystemCPU1ActivePercentage.Should().NotBeNullOrWhiteSpace();
            response.SystemCPU2ActivePercentage.Should().NotBeNull();
            response.SystemCPU3ActivePercentage.Should().NotBeNull();
            response.SystemCPU4ActivePercentage.Should().NotBeNull();
            response.SystemCPUActivePercentage.Should().NotBeNullOrWhiteSpace();
            response.SystemDiskTotalBytes.Should().NotBeNullOrWhiteSpace();
            response.SystemDiskUsedBytes.Should().NotBeNullOrWhiteSpace();
            response.SystemMemoryTotalBytes.Should().NotBeNullOrWhiteSpace();
            response.SystemNetworkReceivedBytes.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryActiveBytes.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryAllocatedbytes.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryFragmentationRatio.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryMappedBytes.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryMetadataBytes.Should().NotBeNullOrWhiteSpace();
            response.TypesenseMemoryResidentBytes.Should().NotBeNullOrWhiteSpace();
            response.TypenseMemoryRetainedBytes.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact, TestPriority(31)]
    public async Task Escape_url_parameters()
    {
        var company = new Company
        {
            Id = "9845",
            CompanyName = "&filter_by=foo",
            NumEmployees = 545,
            Location = new Location
            {
                City = "Paris",
                Country = "FR"
            },
        };

        await _client.CreateDocument<Company>("companies", company);

        var query = new SearchParameters("&filter_by=foo", "company_name");

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(company);
        }
    }

    [Fact, TestPriority(32)]
    public async Task Can_do_vector_search()
    {
        const string COLLECTION_NAME = "address_vector_search";

        try
        {
            var schema = new Schema(
                COLLECTION_NAME,
                new List<Field>
                {
                    new Field("vec", FieldType.FloatArray, 4)
                });

            _ = await _client.CreateCollection(schema);

            await _client.CreateDocument(
                COLLECTION_NAME,
                new AddressVectorSearch()
                {
                    Id = "0",
                    Vec = new float[] { 0.04F, 0.234F, 0.113F, 0.001F }
                });

            // We want to make sure the query works using the query object `VectorQuery`
            var queryUsingQueryObject = new MultiSearchParameters(COLLECTION_NAME, "*")
            {
                // vec:([0.96826, 0.94, 0.39557, 0.306488], k:100)
                VectorQuery = new(
                    vector: [0.96826F, 0.94F, 0.39557F, 0.306488F],
                    vectorFieldName: "vec",
                    k: 100)
            };

            // We want to make sure the query works using a query string
            var queryUsingQueryString = new MultiSearchParameters(COLLECTION_NAME, "*")
            {
                // vec:([0.96826, 0.94, 0.39557, 0.306488], k:100)
                VectorQuery = new("vec:([ 0.96826 , 0.94    ,   0.39557,0.306488],  k : 100 )")
            };

            var queryObjectResponse = await _client.MultiSearch<AddressVectorSearch>(queryUsingQueryObject);
            var queryStringResponse = await _client.MultiSearch<AddressVectorSearch>(queryUsingQueryString);

            using (var scope = new AssertionScope())
            {
                queryObjectResponse.Found.Should().Be(1);
                queryObjectResponse.OutOf.Should().Be(1);
                queryObjectResponse.Page.Should().Be(1);
                queryObjectResponse.Hits.Count.Should().Be(1);
                queryObjectResponse.Hits
                    .First()
                    .Should()
                    .BeEquivalentTo(
                        new Hit<AddressVectorSearch>(
                            new List<Highlight>().AsReadOnly(),
                            new AddressVectorSearch
                            {
                                Id = "0",
                                Vec = new float[] { 0.04F, 0.234F, 0.113F, 0.001F }
                            },
                            null,
                            0.19744956493377686,
                            null));

                queryStringResponse.Found.Should().Be(1);
                queryStringResponse.OutOf.Should().Be(1);
                queryStringResponse.Page.Should().Be(1);
                queryStringResponse.Hits.Count.Should().Be(1);
                queryStringResponse.Hits
                    .First()
                    .Should()
                    .BeEquivalentTo(
                        new Hit<AddressVectorSearch>(
                            new List<Highlight>().AsReadOnly(),
                            new AddressVectorSearch
                            {
                                Id = "0",
                                Vec = new float[] { 0.04F, 0.234F, 0.113F, 0.001F }
                            },
                            null,
                            0.19744956493377686,
                            null));
            }
        }
        finally
        {
            // Make sure that no matter what the collection is deleted.
            try
            {
                _ = _client.DeleteCollection(COLLECTION_NAME);
            }
            catch (TypesenseApiNotFoundException)
            {
                //  Do nothing, it might have crashed before creating the collection.
            }
        }
    }

    [Fact, TestPriority(32)]
    public async Task Search_against_array_field()
    {
        // Give it a random name, to avoid name clashes.
        var collection_name = $"companies-{Guid.NewGuid()}";

        // Setup the schema with a `StringArray` field.
        var schema = new Schema(
            collection_name,
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("aliases", FieldType.StringArray, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("location", FieldType.Object, true),
            },
            "num_employees")
        {
            EnableNestedFields = true
        };

        var company = new Company
        {
            Id = "124",
            CompanyName = "Stark Industries",
            NumEmployees = 5215,
            Location = new Location
            {
                City = "Phoenix",
                Country = "USA"
            },
            Aliases = new string[] { "Stark", "Mickey Stark Inc." }
        };

        try
        {
            _ = await _client.CreateCollection(schema);
            _ = await _client.UpsertDocument(collection_name, company);

            var query = new SearchParameters("Stark", "aliases");

            var result = await _client.Search<Company>(collection_name, query);

            var expectedHighlight = new Highlight(
                "aliases",
                null,
                new List<List<string>>
                {
                    new List<string> { "Stark" },
                        new List<string> { "Stark" }
                },
                new List<string>
                {
                    "<mark>Stark</mark>",
                    "Mickey <mark>Stark</mark> Inc."
                },
                new List<int> { 0, 1 },
                null
            );

            using (var scope = new AssertionScope())
            {
                // Only test for highlight, since it is the one that differs
                result.Hits.First().Highlights.First()
                    .Should()
                    .BeEquivalentTo(expectedHighlight);
            }
        }
        // Make sure that the collection is always cleaned up.
        finally
        {
            await _client.DeleteCollection(collection_name);
        }
    }

    [Fact, TestPriority(33)]
    public async Task Can_do_semantic_search()
    {
        const string COLLECTION_NAME = "course_vector_search";

        try
        {
            var schema = new Schema(
                COLLECTION_NAME,
                new List<Field>
                {
                    new Field("title", FieldType.String),
                    new Field("embedding", FieldType.FloatArray, new AutoEmbeddingConfig(
                        from: new() { "title" },
                        modelConfig: new ModelConfig("ts/all-MiniLM-L12-v2"))
                    ),
                });

            _ = await _client.CreateCollection(schema);

            await _client.CreateDocument(
                COLLECTION_NAME,
                new CourseSemanticSearch
                {
                    Id = "CS101",
                    Title = "Introduction to Programming",
                });

            // We want to make sure the query works using the query object `VectorQuery`
            var queryUsingQueryObject = new MultiSearchParameters(COLLECTION_NAME, "coding", "embedding");

            var queryObjectResponse = await _client.MultiSearch<CourseSemanticSearch>(queryUsingQueryObject);

            using (var scope = new AssertionScope())
            {
                queryObjectResponse.Found.Should().Be(1);
                queryObjectResponse.OutOf.Should().Be(1);
                queryObjectResponse.Page.Should().Be(1);
                queryObjectResponse.Hits.Count.Should().Be(1);
                queryObjectResponse.Hits
                    .First()
                    .Should()
                    .BeEquivalentTo(
                        new Hit<CourseSemanticSearch>(
                            new List<Highlight>().AsReadOnly(),
                            new CourseSemanticSearch
                            {
                                Id = "CS101",
                                Title = "Introduction to Programming",
                            },
                            null,
                            0.4871443510055542,
                            null));
            }
        }
        finally
        {
            // Make sure that no matter what the collection is deleted.
            try
            {
                _ = _client.DeleteCollection(COLLECTION_NAME);
            }
            catch (TypesenseApiNotFoundException)
            {
                //  Do nothing, it might have crashed before creating the collection.
            }
        }
    }

    [Fact, TestPriority(34)]
    public async Task Can_retrieve_health()
    {
        var response = await _client.RetrieveHealth();

        using (var scope = new AssertionScope())
        {
            response.Ok.Should().BeTrue();
        }
    }

    [Fact, TestPriority(35)]
    public async Task Can_create_snapshot()
    {
        var response = await _client.CreateSnapshot("/my_snapshot_path");

        using (var scope = new AssertionScope())
        {
            response.Success.Should().BeTrue();
        }
    }

    [Fact, TestPriority(36)]
    public async Task Can_compact_disk()
    {
        var response = await _client.CompactDisk();

        using (var scope = new AssertionScope())
        {
            response.Success.Should().BeTrue();
        }
    }

    [Fact, TestPriority(37)]
    public async Task Update_document_by_query()
    {
        var document = new
        {
            CompanyName = "Dom Bomb Dot Com",
        };

        var expected = new FilterUpdateResponse(1);
        var response = await _client.UpdateDocuments("companies", document, "num_employees:>100");

        response.Should().BeEquivalentTo(expected);
    }

    private async Task CreateCompanyCollection()
    {
        var schema = new Schema(
            "companies",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("location", FieldType.Object, true),
            },
            "num_employees")
        {
            EnableNestedFields = true
        };

        _ = await _client.CreateCollection(schema);
    }
}
