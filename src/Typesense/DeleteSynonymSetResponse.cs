using System.Text.Json.Serialization;

namespace Typesense;

public record DeleteSynonymSetResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonConstructor]
    public DeleteSynonymSetResponse(string name)
    {
        Name = name;
    }
}
