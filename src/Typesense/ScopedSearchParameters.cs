using System.Text.Json.Serialization;

namespace Typesense;

public record ScopedSearchParameters
{
    /// <summary>
    /// The duration (in seconds) that determines how long the search query is cached.
    /// This value can only be set as part of a scoped API key.
    /// Default: 60
    /// </summary>
    [JsonPropertyName("cache_ttl")]
    public int? CacheTtl { get; init; }
}
