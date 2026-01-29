using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record CurationSetSchema
{
    [JsonPropertyName("items")]
    public IEnumerable<SearchOverrideResponse>? Items { get; init; }

    public CurationSetSchema(IEnumerable<SearchOverrideResponse> items)
    {
        Items = items;
    }
}
