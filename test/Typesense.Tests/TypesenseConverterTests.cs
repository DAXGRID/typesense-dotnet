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

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal(1, response[0].MatchedTokens.Count);
        Assert.True(response[0].MatchedTokens[0] is List<string>);
        Assert.Equal(1, response[1].MatchedTokens.Count);
        Assert.True(response[1].MatchedTokens[0] is string);
    }
}
