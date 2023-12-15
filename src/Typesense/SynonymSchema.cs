using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSchema
{
    [JsonPropertyName("synonyms")]
    public IEnumerable<string> Synonyms { get; init; }

    [JsonPropertyName("root")]
    public string? Root { get; init; }

    [JsonPropertyName("symbols_to_index")]
    public IEnumerable<string>? SymbolsToIndex { get; init; }

    public SynonymSchema(IEnumerable<string> synonyms)
    {
        Synonyms = synonyms;
    }
}
