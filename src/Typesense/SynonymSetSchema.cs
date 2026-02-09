using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSetSchema
{
    [JsonPropertyName("items")]
    public IEnumerable<SynonymSchema>? Items { get; init; }

    public SynonymSetSchema(IEnumerable<SynonymSchema> items)
    {
        Items = items;
    }
}
