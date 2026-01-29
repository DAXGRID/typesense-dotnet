using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSynonymSetsResponse
{
    [JsonPropertyName("synonym_sets")]
    public IReadOnlyCollection<SynonymSetSchemaResponse> SynonymSets { get; init; }

    [JsonConstructor]
    public ListSynonymSetsResponse(IReadOnlyCollection<SynonymSetSchemaResponse> synonymSets)
    {
        SynonymSets = synonymSets;
    }
}
