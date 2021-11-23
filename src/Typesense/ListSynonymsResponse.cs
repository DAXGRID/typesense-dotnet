using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListSynonymsResponse
{
    [JsonPropertyName("synonyms")]
    public IEnumerable<SynonymSchema> Synonyms { get; init; }
}
