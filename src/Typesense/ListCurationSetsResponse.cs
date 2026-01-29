using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListCurationSetsResponse
{
    [JsonPropertyName("curation_sets")]
    public IReadOnlyCollection<CurationSetSchemaResponse> CurationSets { get; init; }

    [JsonConstructor]
    public ListCurationSetsResponse(IReadOnlyCollection<CurationSetSchemaResponse> curationSets)
    {
        CurationSets = curationSets;
    }
}
