using System.Text.Json.Serialization;

namespace Typesense;

public sealed record HealthResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonConstructor]
    public HealthResponse(bool ok)
    {
        Ok = ok;
    }
}
