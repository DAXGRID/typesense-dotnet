using System.Text.Json.Serialization;

namespace Typesense;

public record DeleteCurationSetResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonConstructor]
    public DeleteCurationSetResponse(string name)
    {
        Name = name;
    }
}
