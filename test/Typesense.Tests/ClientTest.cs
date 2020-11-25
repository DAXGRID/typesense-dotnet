using Xunit;
using System.Collections.Generic;
using FluentAssertions;
using System.Threading.Tasks;

namespace Typesense.Tests
{
    public class ClientTest
    {
        [Fact]
        public void ConstructorShouldSetConfigCorrectly()
        {
            var nodes = new List<Node>
            {
                new Node { Host = "localhost:3000", Port = "8108", Protocol = "http" }
            };

            var config = new Config { ApiKey = "1234", ConnectionTimeoutSeconds = 2, Nodes = nodes };

            var client = new Client(config);

            client.Config.Should().BeEquivalentTo(config);
        }

        public async Task CreatCollectionCreatesNewCollection()
        {
            var nodes = new List<Node>
            {
                new Node { Host = "localhost:3000", Port = "8108", Protocol = "http" }
            };

            var config = new Config { ApiKey = "1234", ConnectionTimeoutSeconds = 2, Nodes = nodes };

            var client = new Client(config);

            var schema = new Schema { DefaultSortingField = "num_employees", Name = "companies", NumberOfDocuments = 0, Fields = new List<Field> { new Field { Facet = false, Name = "company_name", Type = "string" } } };

            await client.CreateCollection(schema);
        }
    }
}
