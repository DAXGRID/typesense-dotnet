using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSearchOverridesResponse
{
    [JsonPropertyName("overrides")]
    public IReadOnlyCollection<SearchOverride> SearchOverrides { get; init; }

    [JsonConstructor]
    public ListSearchOverridesResponse(IReadOnlyCollection<SearchOverride> searchOverrides)
    {
        SearchOverrides = searchOverrides;
    }
}
