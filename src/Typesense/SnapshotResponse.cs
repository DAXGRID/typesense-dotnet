using System.Text.Json.Serialization;

namespace Typesense;

public record SnapshotResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonConstructor]
    public SnapshotResponse(bool success)
    {
        Success = success;
    }
}