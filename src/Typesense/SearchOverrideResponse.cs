using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SearchOverrideResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("excludes")]
    public IEnumerable<Exclude> Excludes { get; init; }
    [JsonPropertyName("includes")]
    public IEnumerable<Include> Includes { get; init; }
    [JsonPropertyName("rule")]
    public Rule Rule { get; init; }

    [JsonConstructor]
    public SearchOverrideResponse(IEnumerable<Exclude> excludes, IEnumerable<Include> includes, Rule rule, string id)
    {
        Excludes = excludes;
        Includes = includes;
        Rule = rule;
        Id = id;
    }
}
