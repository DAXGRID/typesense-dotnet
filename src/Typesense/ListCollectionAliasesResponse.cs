using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListCollectionAliasesResponse
{
    [JsonPropertyName("aliases")]
    public IEnumerable<CollectionAlias> CollectionAliases { get; init; }
}
