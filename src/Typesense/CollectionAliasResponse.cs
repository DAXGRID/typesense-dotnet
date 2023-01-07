using System.Text.Json.Serialization;

namespace Typesense;

public record CollectionAliasResponse
{
    [JsonPropertyName("collection_name")]
    public string CollectionName { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonConstructor]
    public CollectionAliasResponse(string collectionName, string name)
    {
        CollectionName = collectionName;
        Name = name;
    }
}
