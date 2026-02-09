using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record CurationSetSchemaResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("items")]
    public IEnumerable<SearchOverride>? Items { get; init; }

    public CurationSetSchemaResponse(string name, IEnumerable<SearchOverride> items)
    {
        Name = name;
        Items = items;
    }
}
