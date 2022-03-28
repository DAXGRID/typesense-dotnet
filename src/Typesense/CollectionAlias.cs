using System;
using System.Text.Json.Serialization;

namespace Typesense;

public record CollectionAlias
{
    [JsonPropertyName("collection_name")]
    public string CollectionName { get; init; }

    public CollectionAlias(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException(
                $"{nameof(collectionName)} cannot be null, empty or whitespace.");
        CollectionName = collectionName;
    }
}
