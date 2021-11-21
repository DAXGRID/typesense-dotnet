using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;
public record Exclude
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonConstructor]
    public Exclude(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(id)} cannot be null or empty.");
        Id = id;
    }
}

public record Include
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("position")]
    public int Position { get; init; }

    [JsonConstructor]
    public Include(string id, int position)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException($"{nameof(id)} cannot be null or empty.");
        Id = id;
        Position = position;
    }
}

public record Rule
{
    [JsonPropertyName("query")]
    public string Query { get; init; }
    [JsonPropertyName("match")]
    public string Match { get; init; }

    [JsonConstructor]
    public Rule(string query, string match)
    {
        if (string.IsNullOrEmpty(query))
            throw new ArgumentException($"{nameof(query)} cannot be null or empty.");
        if (string.IsNullOrEmpty(match))
            throw new ArgumentException($"{nameof(match)} cannot be null or empty.");
        Match = match;
        Query = query;
    }
}

public record SearchOverride
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("excludes")]
    public IEnumerable<Exclude> Excludes { get; init; }
    [JsonPropertyName("includes")]
    public IEnumerable<Include> Includes { get; init; }
    [JsonPropertyName("rule")]
    public Rule Rule { get; init; }

    public SearchOverride(List<Include> includes, Rule rule)
    {
        if (rule is null)
            throw new ArgumentNullException($"{nameof(rule)} cannot be null.");
        Includes = includes;
        Rule = rule;
    }

    public SearchOverride(IEnumerable<Exclude> excludes, Rule rule)
    {
        if (rule is null)
            throw new ArgumentNullException($"{nameof(rule)} cannot be null.");
        Excludes = excludes;
        Rule = rule;
    }

    public SearchOverride(IEnumerable<Exclude> excludes, IEnumerable<Include> includes, Rule rule)
    {
        if (rule is null)
            throw new ArgumentNullException($"{nameof(rule)} cannot be null.");
        Excludes = excludes;
        Includes = includes;
        Rule = rule;
    }

    [JsonConstructor]
    public SearchOverride(IEnumerable<Exclude> excludes, IEnumerable<Include> includes, Rule rule, string id)
    {
        Excludes = excludes;
        Includes = includes;
        Rule = rule;
        Id = id;
    }
}
