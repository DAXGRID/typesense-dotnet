using System.Text.Json.Serialization;

namespace Typesense;

public record TruncateCollectionResponse
{
    [JsonPropertyName("num_deleted")]
    public int NumDeleted { get; init; }

    [JsonConstructor]
    public TruncateCollectionResponse(int numDeleted)
    {
        NumDeleted = numDeleted;
    }
}
