using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.Linq;
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
            Country = "USA",
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
            Country = "USA",
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
            Country = "SWE",
        };

        var response = await _client.UpsertDocument<Company>("companies", company);

        response.Should().BeEquivalentTo(company);
    }

    [Fact, TestPriority(7)]
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
                Country = "USA",
            },
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
            },
            new Company
            {
                Id = "999",
                CompanyName = "Awesome A/S",
                NumEmployees = 10,
                Country = "SWE",
            }
        };

        var response = await _client.ExportDocuments<Company>("companies");

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
            Country = "USA",
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
            Country = "USA",
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
            Country = "USA",
        };

        var query = new SearchParameters
        {
            Text = "Stark",
            QueryBy = "company_name"
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
            Country = "USA",
        };

        var query = new SearchParameters
        {
            Text = "Stark",
            QueryBy = "company_name,country"
        };

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
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
            Country = "USA",
        };

        var response = await _client.DeleteDocument<Company>("companies", "124");

        response.Should().BeEquivalentTo(response);
    }

    [Fact, TestPriority(13)]
    public async Task Delete_document_by_query()
    {
        var expected = new FilterDeleteResponse { NumberOfDeleted = 2 };
        var response = await _client.DeleteDocuments("companies", "num_employees:>100");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(14)]
    public async Task Create_api_key()
    {
        var expected = new Key()
        {
            Description = "Example key one",
            Actions = new[] { "*" },
            Collections = new[] { "*" },
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
        var expected = new Key()
        {
            Description = "Example key one",
            Actions = new[] { "*" },
            Collections = new[] { "*" },
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
            new List<Include>
            {
                new Include("422", 1),
                new Include("54", 2)
            },
            new Rule("apple", "exact"));

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
            new List<Include>
            {
                new Include("422", 1),
                new Include("54", 2)
            },
            new Rule("apple", "exact"));

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
        var expected = new SearchOverride(
            new List<Include>
            {
                new Include("422", 1),
                new Include("54", 2)
            },
            new Rule("apple", "exact"));

        var response = await _client.DeleteSearchOverride("companies", "customize-apple");

        response.Id.Should().BeEquivalentTo("customize-apple");
    }

    [Fact, TestPriority(22)]
    [Trait("Category", "Integration")]
    public async Task Upsert_collection_alias()
    {
        var expected = new CollectionAlias("companies", "my-companies-alias");

        var response = await _client.UpsertCollectionAlias(
            "my-companies-alias", new CollectionAlias("companies"));

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(23)]
    public async Task List_collection_aliases()
    {
        var expected = new CollectionAlias("companies", "my-companies-alias");

        var response = await _client.ListCollectionAliases();

        response.CollectionAliases.Should()
            .HaveCount(1).And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().BeEquivalentTo(expected);
                });
    }

    [Fact, TestPriority(24)]
    public async Task Retrieve_collection_alias()
    {
        var expected = new CollectionAlias("companies", "my-companies-alias");

        var response = await _client.RetrieveCollectionAlias("my-companies-alias");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(25)]
    public async Task Delete_collection_alias()
    {
        var expected = new CollectionAlias("companies", "my-companies-alias");

        var response = await _client.DeleteCollectionAlias("my-companies-alias");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(26)]
    public async Task Upsert_synonym()
    {
        var expected = new SynonymSchemaResponse
        {
            Id = "apple-synonyms",
            Root = "apple",
            Synonyms = new List<string> { "appl", "aple", "apple" }
        };

        var schema = new SynonymSchema(new List<string> { "appl", "aple", "apple" }, "apple");
        var response = await _client.UpsertSynonym("companies", "apple-synonyms", schema);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(27)]
    public async Task Retrieve_synonym()
    {
        var expected = new SynonymSchemaResponse
        {
            Id = "apple-synonyms",
            Root = "apple",
            Synonyms = new List<string> { "appl", "aple", "apple" }
        };

        var response = await _client.RetrieveSynonym("companies", "apple-synonyms");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(28)]
    public async Task List_synonyms()
    {
        var expected = new ListSynonymsResponse
        {
            Synonyms = new List<SynonymSchemaResponse>
            {
                new SynonymSchemaResponse
                {
                    Id = "apple-synonyms",
                    Root = "apple",
                    Synonyms = new List<string> { "appl", "aple", "apple" }
                }
            }
        };

        var response = await _client.ListSynonyms("companies");

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(29)]
    public async Task Delete_synonym()
    {
        var expected = new DeleteSynonymResponse { Id = "apple-synonyms" };
        var response = await _client.DeleteSynonym("companies", "apple-synonyms");

        response.Should().Be(expected);
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
