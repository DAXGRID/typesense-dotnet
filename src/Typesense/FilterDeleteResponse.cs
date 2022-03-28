using System.Text.Json.Serialization;

namespace Typesense;

public record FilterDeleteResponse
{
    [JsonPropertyName("num_deleted")]
    public int NumberOfDeleted { get; init; }

    [JsonConstructor]
    public FilterDeleteResponse(int numberOfDeleted)
    {
        NumberOfDeleted = numberOfDeleted;
    }
}
