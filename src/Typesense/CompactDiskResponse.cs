using System.Text.Json.Serialization;

namespace Typesense;

public record CompactDiskResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonConstructor]
    public CompactDiskResponse(bool success)
    {
        Success = success;
    }
}