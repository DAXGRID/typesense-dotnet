using System.Text.Json.Serialization;

namespace Typesense;

public record DeleteSynonymResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonConstructor]
    public DeleteSynonymResponse(string id)
    {
        Id = id;
    }
}
