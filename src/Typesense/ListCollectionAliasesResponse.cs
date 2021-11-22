using System.Collections.Generic;

namespace Typesense;
public record ListCollectionAliasesResponse
{
    public IEnumerable<CollectionAlias> CollectionAliases { get; init; }
}
