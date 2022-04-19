using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Typesense.Tests;

[Trait("Category", "Unit")]
public class TypesenseConverterTests
{
    [Fact]
    public void TestMixedTokens()
    {
        var json = @"
        [
            {
                ""field"": ""tags"",
                ""indices"": [
                    0
                ],
                ""matched_tokens"": [
                    [
                        ""Bar""
                    ]
                ],
                ""snippets"": [
                    ""<mark>Bar</mark>""
                ]
            },
            {
                ""field"": ""title"",
                ""matched_tokens"": [
                    ""Bar""
                ],
                ""snippet"": ""Wayward Star <mark>Bar</mark>""
            }
        ]
        ";

        var response = JsonSerializer.Deserialize<Highlight[]>(json);

        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Length.Should().Be(2);
            response[0].MatchedTokens.Count.Should().Be(1);
            response[0].MatchedTokens[0].Should().BeOfType<List<string>>();
            response[1].MatchedTokens.Count.Should().Be(1);
            response[1].MatchedTokens[0].Should().BeOfType<string>();
        }
    }
}
