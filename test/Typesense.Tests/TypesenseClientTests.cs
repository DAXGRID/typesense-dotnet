using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Typesense.Setup;
using Xunit;

namespace Typesense.Tests;

public class TypesenseClientTests : IClassFixture<TypesenseFixture>
{
    [Fact]
    public async Task Create_schema()
    {
        var expected = new CollectionResponse
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

        var client = GetClient();

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

        var response = await client.CreateCollection(schema);

        response.Should().BeEquivalentTo(expected);

        // Cleanup
        await client.DeleteCollection(schema.Name);
    }

    private async Task DeleteCollection(string name)
    {
        var client = GetClient();
        await client.DeleteCollection(name);
    }

    private ITypesenseClient GetClient()
    {
        return new ServiceCollection()
            .AddTypesenseClient(config =>
            {
                config.ApiKey = "key";
                config.Nodes = new List<Node> { new Node { Host = "localhost", Port = "8108", Protocol = "http" } };
            }).BuildServiceProvider().GetService<ITypesenseClient>();
    }
}
