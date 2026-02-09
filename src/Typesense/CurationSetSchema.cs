using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record CurationSetSchema
{
    [JsonPropertyName("items")]
    public IEnumerable<SearchOverride>? Items { get; init; }

    public CurationSetSchema(IEnumerable<SearchOverride> items)
    {
        Items = items;
    }
}
