using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSearchOverridesResponse
{
    [JsonPropertyName("overrides")]
    public IReadOnlyCollection<SearchOverrideResponse> SearchOverrides { get; init; }

    [JsonConstructor]
    public ListSearchOverridesResponse(IReadOnlyCollection<SearchOverrideResponse> searchOverrides)
    {
        SearchOverrides = searchOverrides;
    }
}
