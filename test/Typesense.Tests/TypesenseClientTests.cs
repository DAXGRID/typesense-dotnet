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
        var expected = new CollectionResponse(
            "companies",
            0,
            new List<Field>
            {
                new Field("company_name",
                    type: FieldType.String,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
                new Field("num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false),
                new Field(
                    name: "country",
                    type: FieldType.String,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
            },
            "num_employees",
            new List<string>(),
            new List<string>());

        var schema = new Schema(
            "companies",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("country", FieldType.String, true),
            },
            "num_employees");

        var response = await _client.CreateCollection(schema);

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
                    infix: false),
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false),
                new Field(
                    name: "country",
                    type: FieldType.String,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
            },
            "num_employees",
            new List<string> { "-" },
            new List<string> { "+" });

        var schema = new Schema(
            "companies_with_symbols_and_token",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("country", FieldType.String, true),
            },
            "num_employees")
        {
            TokenSeparators = new List<string> { "-" },
            SymbolsToIndex = new List<string> { "+" }
        };

        var response = await _client.CreateCollection(schema);

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
                    infix: false),
            },
            "",
            new List<string>(),
            new List<string>());

        var schema = new Schema(
            "wildcard-collection",
            new List<Field>
            {
                new Field(".*", FieldType.String, false),
            });

        var response = await _client.CreateCollection(schema);

        response.Should().BeEquivalentTo(expected);

        // Cleanup
        await _client.DeleteCollection("wildcard-collection");
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
                    infix: false),
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false),
                new Field(
                    "country",
                    type: FieldType.String,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
            },
            "num_employees",
            new List<string>(),
            new List<string>());

        var response = await _client.RetrieveCollection("companies");

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
                        infix: false),
                    new Field(
                        name: "num_employees",
                        type: FieldType.Int32,
                        facet: true,
                        optional: false,
                        index: true,
                        sort: true,
                        infix: false),
                    new Field(
                        name: "country",
                        type: FieldType.String,
                        facet: true,
                        optional: false,
                        index: true,
                        sort: false,
                        infix: false),
                },
                "num_employees",
                new List<string>(),
                new List<string>())
    };

        var response = await _client.RetrieveCollections();
        response.Should().BeEquivalentTo(expected);
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
                    name: "company_name",
                    type: FieldType.String,
                    facet: false,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
                new Field(
                    name: "num_employees",
                    type: FieldType.Int32,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: true,
                    infix: false),
                new Field(
                    name: "country",
                    type: FieldType.String,
                    facet: true,
                    optional: false,
                    index: true,
                    sort: false,
                    infix: false),
            },
            "num_employees",
            new List<string>(),
            new List<string>());

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
            "companies", companies, 40, ImportType.Create);

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
            "companies", companies, 40, ImportType.Emplace);

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
                Country = "USA",
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
                Country = "USA",
            },
            new Company
            {
                Id = "125",
                CompanyName = "Future Technology",
                Country = "UK",
            },
            new Company
            {
                Id = "126",
                CompanyName = "Random Corp.",
                Country = "AU",
            },
            new Company
            {
                Id = "999",
                CompanyName = "Awesome A/S",
                Country = "SWE",
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

        var query = new SearchParameters("Stark", "company_name");

        var response = await _client.Search<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.Found.Should().Be(1);
            response.Hits.First().Document.Should().BeEquivalentTo(expected);
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
            Country = "USA",
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
            Country = "USA",
        };

        var query = new SearchParameters("Stark", "company_name,country");
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
        var expected = new FacetCount("country", new List<FacetCountHit>
            {
                new FacetCountHit( "AU", 1, "AU"),
                new FacetCountHit( "UK", 1, "UK"),
                new FacetCountHit( "SWE", 1, "SWE"),
                new FacetCountHit( "USA", 1, "USA"),
            }, new FacetStats(0, 0, 0, 0, 4));

        var query = new SearchParameters("", "company_name")
        {
            FacetBy = "country"
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
        var expected = new FacetCount("country", new List<FacetCountHit>
            {
                new FacetCountHit("USA", 1, "USA"),
            }, new FacetStats(0, 0, 0, 0, 1));

        var query = new SearchParameters("Stark", "company_name")
        {
            FacetBy = "country"
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
        var query = new GroupedSearchParameters("Stark", "company_name", "country");
        var response = await _client.SearchGrouped<Company>("companies", query);

        using (var scope = new AssertionScope())
        {
            response.GroupedHits.Should().NotBeEmpty();
            var firstHit = response.GroupedHits.First();
            firstHit.GroupKey.Should().BeEquivalentTo("USA");
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
            Country = "USA",
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
            Country = "USA",
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
            Country = "USA",
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
            Country = "USA",
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
            Country = "USA",
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
            "apple");

        var schema = new SynonymSchema(new List<string> { "appl", "aple", "apple" }) { Root = "apple" };
        var response = await _client.UpsertSynonym("companies", "apple-synonyms", schema);

        response.Should().BeEquivalentTo(expected);
    }

    [Fact, TestPriority(27)]
    public async Task Retrieve_synonym()
    {
        var expected = new SynonymSchemaResponse(
            "apple-synonyms",
            new List<string> { "appl", "aple", "apple" },
            "apple");

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
                    "apple")
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
            Country = "FR",
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

    private async Task CreateCompanyCollection()
    {
        var schema = new Schema(
            "companies",
            new List<Field>
            {
                new Field("company_name", FieldType.String, false),
                new Field("num_employees", FieldType.Int32, true),
                new Field("country", FieldType.String, true),
            },
            "num_employees");

        _ = await _client.CreateCollection(schema);
    }
}
