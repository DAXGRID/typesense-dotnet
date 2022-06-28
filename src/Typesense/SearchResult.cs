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
    public string Snippet { get; init; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage
    ("Naming", "CA1721: Property names should not match get methods",
     Justification = "Required because of special case regarding matched tokens.")]
    [JsonPropertyName("matched_tokens")]
    [JsonConverter(typeof(MatchedTokenConverter))]
    public IReadOnlyList<object> MatchedTokens { get; init; }
    public IReadOnlyList<T> GetMatchedTokens<T>() => MatchedTokens.Cast<T>().ToList();

    public Highlight(string field, string snippet, IReadOnlyList<object> matchedTokens)
    {
        Field = field;
        Snippet = snippet;
        MatchedTokens = matchedTokens;
    }
}

public record Hit<T>
{
    [JsonPropertyName("highlights")]
    public IReadOnlyList<Highlight> Highlights { get; init; }
    [JsonPropertyName("document")]
    public T Document { get; init; }
    [JsonPropertyName("text_match")]
    public long TextMatch { get; init; }

    public Hit(IReadOnlyList<Highlight> highlights, T document, long textMatch)
    {
        Highlights = highlights;
        Document = document;
        TextMatch = textMatch;
    }
}

public record SearchResult<T>
{
    [JsonPropertyName("facet_counts")]
    public IReadOnlyCollection<int> FacetCounts { get; init; }
    [JsonPropertyName("found")]
    public int Found { get; init; }
    [JsonPropertyName("out_of")]
    public int OutOf { get; init; }
    [JsonPropertyName("page")]
    public int Page { get; init; }
    [JsonPropertyName("search_time_ms")]
    public int SearchTimeMs { get; init; }
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit<T>> Hits { get; init; }
    [JsonPropertyName("took_ms")]
    [Obsolete("Obsolete since version v0.18.0 use SearchTimeMs instead.")]
    public int? TookMs { get; init; }

    [JsonConstructor]
    public SearchResult(
        IReadOnlyCollection<int> facetCounts,
        int found,
        int outOf,
        int page,
        int searchTimeMs,
        IReadOnlyList<Hit<T>> hits)
    {
        FacetCounts = facetCounts;
        Found = found;
        OutOf = outOf;
        Page = page;
        SearchTimeMs = searchTimeMs;
        Hits = hits;
    }
}

public record MultiSearchResult
{
    public IEnumerable<SearchResult<object>> SearchResults { get; init; }

    public MultiSearchResult(IEnumerable<SearchResult<object>> searchResults)
    {
        SearchResults = searchResults;
    }
}
