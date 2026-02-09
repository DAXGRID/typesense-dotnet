using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSchema
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("synonyms")]
    public IEnumerable<string> Synonyms { get; init; }

    [JsonPropertyName("root")]
    public string? Root { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("symbols_to_index")]
    public IEnumerable<string>? SymbolsToIndex { get; init; }

    public SynonymSchema(string id, IEnumerable<string> synonyms, string? root = null, string? locale = null, IEnumerable<string>? symbolsToIndex = null)
    {
        Id = id;
        Synonyms = synonyms;
        Root = root;
        Locale = locale;
        SymbolsToIndex = symbolsToIndex;
    }
}
