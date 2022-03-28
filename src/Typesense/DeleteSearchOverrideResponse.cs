using System.Text.Json.Serialization;

namespace Typesense;

public record DeleteSearchOverrideResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonConstructor]
    public DeleteSearchOverrideResponse(string id)
    {
        Id = id;
    }
}
