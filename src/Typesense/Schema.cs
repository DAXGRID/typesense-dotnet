using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record Schema
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("fields")]
    public IEnumerable<Field> Fields { get; init; }
    [JsonPropertyName("default_sorting_field")]
    public string DefaultSortingField { get; init; }
}
