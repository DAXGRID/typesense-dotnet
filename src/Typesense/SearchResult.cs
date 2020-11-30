using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public record Highlight
    {
        [JsonPropertyName("field")]
        public string Field { get; init; }
        [JsonPropertyName("snippet")]
        public string Snippet { get; init; }
        [JsonPropertyName("matched_tokens")]
        public IReadOnlyList<string> MatchedTokens { get; init; }
    }

    public record Hit<T>
    {
        [JsonPropertyName("highlights")]
        public IReadOnlyList<Highlight> Highlights { get; init; }
        [JsonPropertyName("document")]
        public T Document { get; init; }
    }

    public record SearchResult<T>
    {
        [JsonPropertyName("facet_counts")]
        public IReadOnlyList<object> FacetCounts { get; init; }
        [JsonPropertyName("found")]
        public int Found { get; init; }
        [JsonPropertyName("took_ms")]
        public int TookMs { get; init; }
        [JsonPropertyName("hits")]
        public IReadOnlyList<Hit<T>> Hits { get; init; }
    }
}
