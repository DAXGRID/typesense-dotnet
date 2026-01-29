using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSetSchemaResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("items")]
    public IEnumerable<SynonymSchemaResponse>? Items { get; init; }

    public SynonymSetSchemaResponse(string name, IEnumerable<SynonymSchemaResponse> items)
    {
        Name = name;
        Items = items;
    }
}
