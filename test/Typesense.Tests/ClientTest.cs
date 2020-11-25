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

            var config = new Config { ApiKey = "1234", Nodes = nodes };

            var client = new Client(config);

            client.Config.Should().BeEquivalentTo(config);
        }
    }
}
