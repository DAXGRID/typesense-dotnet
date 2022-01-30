using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSchemaResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("synonyms")]
    public IEnumerable<string> Synonyms { get; init; }
    [JsonPropertyName("root")]
    public string Root { get; init; }
}
