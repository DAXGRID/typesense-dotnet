using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSynonymsResponse
{
    [JsonPropertyName("synonyms")]
    public IReadOnlyCollection<SynonymSchemaResponse> Synonyms { get; init; }

    [JsonConstructor]
    public ListSynonymsResponse(IReadOnlyCollection<SynonymSchemaResponse> synonyms)
    {
        Synonyms = synonyms;
    }
}
