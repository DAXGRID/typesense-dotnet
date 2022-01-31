using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public class Schema
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("fields")]
    public List<Field> Fields { get; set; }
    [JsonPropertyName("default_sorting_field")]
    public string DefaultSortingField { get; set; }
}
