using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSetSchema
{
    [JsonPropertyName("items")]
    public IEnumerable<SynonymSchemaResponse>? Items { get; init; }

    public SynonymSetSchema(IEnumerable<SynonymSchemaResponse> items)
    {
        Items = items;
    }
}
