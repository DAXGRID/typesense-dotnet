using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public class Highlight
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }
        [JsonPropertyName("snippet")]
        public string Snippet { get; set; }
        [JsonPropertyName("matched_tokens")]
        public IReadOnlyList<string> MatchedTokens { get; set; }
    }

    public class Hit<T>
    {
        [JsonPropertyName("highlights")]
        public IReadOnlyList<Highlight> Highlights { get; set; }
        [JsonPropertyName("document")]
        public T Document { get; set; }
    }

    public class SearchResult<T>
    {
        [JsonPropertyName("facet_counts")]
        public IReadOnlyList<object> FacetCounts { get; set; }
        [JsonPropertyName("found")]
        public int Found { get; set; }
        [JsonPropertyName("took_ms")]
        public int TookMs { get; set; }
        [JsonPropertyName("hits")]
        public IReadOnlyList<Hit<T>> Hits { get; set; }
    }
}
