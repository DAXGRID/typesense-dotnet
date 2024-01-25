using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public record Highlight
{
    [JsonPropertyName("field")]
    public string Field { get; init; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; init; }

    [JsonPropertyName("snippets")]
    public IReadOnlyList<string>? Snippets { get; init; }

    [JsonPropertyName("indices")]
    public IReadOnlyList<int>? Indices { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage
    ("Naming", "CA1721: Property names should not match get methods",
     Justification = "Required because of special case regarding matched tokens.")]
    [JsonPropertyName("matched_tokens")]
    [JsonConverter(typeof(MatchedTokenConverter))]
    public IReadOnlyList<object> MatchedTokens { get; init; }

    public IReadOnlyList<T> GetMatchedTokens<T>() => MatchedTokens.Cast<T>().ToList();

    public Highlight(
        string field,
        string? snippet,
        IReadOnlyList<object> matchedTokens,
        IReadOnlyList<string>? snippets,
        IReadOnlyList<int>? indices,
        string? value)
    {
        Field = field;
        Snippet = snippet;
        MatchedTokens = matchedTokens;
        Snippets = snippets;
        Indices = indices;
        Value = value;
    }
}

public record Hit<T>
{
    [JsonPropertyName("highlights")]
    public IReadOnlyList<Highlight> Highlights { get; init; }

    [JsonPropertyName("document")]
    public T Document { get; init; }

    [JsonPropertyName("text_match")]
    public long? TextMatch { get; init; }

    [JsonPropertyName("vector_distance")]
    public double? VectorDistance { get; init; }

    [JsonConstructor]
    public Hit(IReadOnlyList<Highlight> highlights, T document, long? textMatch, double? vectorDistance)
    {
        Highlights = highlights;
        Document = document;
        TextMatch = textMatch;
        VectorDistance = vectorDistance;
    }
}

public record FacetCount
{
    [JsonPropertyName("counts")]
    public IReadOnlyList<FacetCountHit> Counts { get; init; }

    [JsonPropertyName("field_name")]
    public string FieldName { get; init; }

    [JsonPropertyName("stats")]
    public FacetStats Stats { get; init; }

    public FacetCount(string fieldName, IReadOnlyList<FacetCountHit> counts, FacetStats stats)
    {
        FieldName = fieldName;
        Counts = counts;
        Stats = stats;
    }
}

public record FacetCountHit
{
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("highlighted")]
    public string Highlighted { get; init; }

    [JsonPropertyName("value")]
    public string Value { get; init; }

    public FacetCountHit(string value, int count, string highlighted)
    {
        Value = value;
        Count = count;
        Highlighted = highlighted;
    }
}

public record FacetStats
{
    [JsonPropertyName("avg")]
    public float Average { get; init; }

    [JsonPropertyName("max")]
    public float Max { get; init; }

    [JsonPropertyName("min")]
    public float Min { get; init; }

    [JsonPropertyName("sum")]
    public float Sum { get; init; }

    [JsonPropertyName("total_values")]
    public int TotalValues { get; init; }

    [JsonConstructor]
    public FacetStats(
        float average,
        float max,
        float min,
        float sum,
        int totalValues)
    {
        Average = average;
        Max = max;
        Min = min;
        Sum = sum;
        TotalValues = totalValues;
    }
}

public record GroupedHit<T>
{
    [JsonPropertyName("group_key")]
    [JsonConverter(typeof(GroupKeyConverter))]
    public IReadOnlyList<string> GroupKey { get; init; }

    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit<T>> Hits { get; init; }

    [JsonConstructor]
    public GroupedHit(IReadOnlyList<string> groupKey, IReadOnlyList<Hit<T>> hits)
    {
        GroupKey = groupKey;
        Hits = hits;
    }
}

public abstract record SearchResultBase
{
    [JsonPropertyName("facet_counts")]
    public IReadOnlyCollection<FacetCount> FacetCounts { get; init; }

    [JsonPropertyName("found")]
    public int Found { get; init; }

    [JsonPropertyName("found_docs")]
    public int? FoundDocs { get; init; }

    [JsonPropertyName("out_of")]
    public int OutOf { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("search_time_ms")]
    public int SearchTimeMs { get; init; }

    [JsonPropertyName("took_ms")]
    [Obsolete("Obsolete since version v0.18.0 use SearchTimeMs instead.")]
    public int? TookMs { get; init; }

    [JsonConstructor]
    protected SearchResultBase(
        IReadOnlyCollection<FacetCount> facetCounts,
        int found,
        int outOf,
        int page,
        int searchTimeMs,
        int? tookMs,
        int? foundDocs = null)
    {
        FacetCounts = facetCounts;
        Found = found;
        OutOf = outOf;
        Page = page;
        SearchTimeMs = searchTimeMs;
        TookMs = tookMs;
        FoundDocs = foundDocs;
    }
}

public record SearchResult<T> : SearchResultBase
{
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit<T>> Hits { get; init; }

    [JsonConstructor]
    public SearchResult(
        IReadOnlyCollection<FacetCount> facetCounts,
        int found,
        int outOf,
        int page, int searchTimeMs,
        int? tookMs,
        IReadOnlyList<Hit<T>> hits) : base(facetCounts, found, outOf, page, searchTimeMs, tookMs)
    {
        Hits = hits;
    }
}

public record SearchGroupedResult<T> : SearchResultBase
{
    [JsonPropertyName("grouped_hits")]
    public IReadOnlyList<GroupedHit<T>> GroupedHits { get; init; }

    [JsonConstructor]
    public SearchGroupedResult(
        IReadOnlyCollection<FacetCount> facetCounts,
        int found,
        int outOf,
        int page,
        int searchTimeMs,
        int? tookMs,
        IReadOnlyList<GroupedHit<T>> groupedHits
    ) : base(facetCounts, found, outOf, page, searchTimeMs, tookMs)
    {
        GroupedHits = groupedHits;
    }
}

public record MultiSearchResult<T>
{
    [JsonPropertyName("facet_counts")]
    public IReadOnlyList<FacetCount>? FacetCounts { get; init; }

    [JsonPropertyName("found")]
    public int? Found { get; init; }

    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit<T>>? Hits { get; init; }

    [JsonPropertyName("out_of")]
    public int? OutOf { get; init; }

    [JsonPropertyName("page")]
    public int? Page { get; init; }

    [JsonPropertyName("search_cutoff")]
    public bool? SearchCutoff { get; init; }

    [JsonPropertyName("search_time_ms")]
    public int? SearchTimeMs { get; init; }

    [JsonPropertyName("code")]
    public int? ErrorCode { get; init; }

    [JsonPropertyName("error")]
    public string? ErrorMessage { get; init; }

    [JsonConstructor]
    public MultiSearchResult(
        IReadOnlyList<FacetCount>? facetCounts,
        int? found,
        IReadOnlyList<Hit<T>>? hits,
        int? outOf,
        int? page,
        bool? searchCutoff,
        int? searchTimeMs,
        int? errorCode,
        string? errorMessage)
    {
        FacetCounts = facetCounts;
        Found = found;
        Hits = hits;
        OutOf = outOf;
        Page = page;
        SearchCutoff = searchCutoff;
        SearchTimeMs = searchTimeMs;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
