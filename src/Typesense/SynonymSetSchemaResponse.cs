using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSetSchemaResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("items")]
    public IEnumerable<SynonymSchema>? Items { get; init; }

    public SynonymSetSchemaResponse(string name, IEnumerable<SynonymSchema> items)
    {
        Name = name;
        Items = items;
    }
}
