using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListCollectionAliasesResponse
{
    [JsonPropertyName("aliases")]
    public IReadOnlyCollection<CollectionAliasResponse> CollectionAliases { get; init; }

    [JsonConstructor]
    public ListCollectionAliasesResponse(IReadOnlyCollection<CollectionAliasResponse> collectionAliases)
    {
        CollectionAliases = collectionAliases;
    }
}
