using System.Text.Json.Serialization;

namespace Typesense;

public record ExportParameters
{
    [JsonPropertyName("filter_by")]
    public string? FilterBy { get; set; }
    
    [JsonPropertyName("include_fields")]
    public string? IncludeFields { get; set; }
    
    [JsonPropertyName("exclude_fields")]
    public string? ExcludeFields { get; set; }
}
