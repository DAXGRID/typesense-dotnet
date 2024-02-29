using System.Text.Json.Serialization;

namespace Typesense;

public record FilterUpdateResponse
{
    [JsonPropertyName("num_updated")]
    public int NumberUpdated { get; init; }

    [JsonConstructor]
    public FilterUpdateResponse(int numberUpdated)
    {
        NumberUpdated = numberUpdated;
    }
}
