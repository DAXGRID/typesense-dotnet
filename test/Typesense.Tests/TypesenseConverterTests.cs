using System;
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

    [Fact]
    public void TestGroupKeys()
    {
        var json = @"
        {
          ""found"": 36,
          ""group_key"": [""USA"", 12, 4.2, [""foo"", ""bar""], [42, 4.5]],
          ""Hits"": []
        }
        ";

        var groupedHit = JsonSerializer.Deserialize<GroupedHit<string>>(json);

        using (new AssertionScope())
        {
            groupedHit.Should().NotBeNull();
            groupedHit.Found.Should().Be(36);

            var key = groupedHit.GroupKey;
            key.Count.Should().Be(5);

            key[0].Should().Be("USA");
            key[1].Should().Be("12");
            key[2].Should().Be("4.2");
            key[3].Should().Be("foo, bar");
            key[4].Should().Be("42, 4.5");
        }
    }

    [Fact]
    public void VectorQueryConstructor_AllowsEmptyVectorWithoutId()
    {
        var query = new VectorQuery(Array.Empty<float>(), "embedding");

        using (new AssertionScope())
        {
            query.VectorFieldName.Should().Be("embedding");
            query.Vector.Should().BeEmpty();
            query.Id.Should().BeNull();
            query.ToQuery().Should().Be("embedding:([])");
        }
    }

    [Fact]
    public void VectorQueryParseQuery_ParsesAlphaAndDistanceThresholdOnEmptyVector()
    {
        var query = new VectorQuery("embedding:([], alpha: 0.7, distance_threshold:0.30)");

        using (new AssertionScope())
        {
            query.VectorFieldName.Should().Be("embedding");
            query.Vector.Should().BeEmpty();
            query.Alpha.Should().Be(0.7m);
            query.DistanceThreshold.Should().Be(0.30m);
            query.Id.Should().BeNull();
        }
    }

    [Fact]
    public void VectorQueryToQuery_IncludesAlphaAndDistanceThreshold()
    {
        var query = new VectorQuery(
            vector: Array.Empty<float>(),
            vectorFieldName: "embedding",
            alpha: 0.7m,
            distanceThreshold: 0.30m);

        query.ToQuery().Should().Be("embedding:([],alpha:0.7,distance_threshold:0.30)");
    }
}
