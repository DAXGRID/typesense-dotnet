using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSearchOverridesResponse
{
    [JsonPropertyName("overrides")]
    public IEnumerable<SearchOverride> SearchOverrides { get; init; }
}
