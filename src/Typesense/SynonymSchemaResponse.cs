using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSchemaResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("synonyms")]
    public IReadOnlyCollection<string> Synonyms { get; init; }

    [JsonPropertyName("root")]
    public string? Root { get; init; }

    [JsonPropertyName("symbols_to_index")]
    public IReadOnlyCollection<string>? SymbolsToIndex { get; init; }

    [JsonConstructor]
    public SynonymSchemaResponse(string id,
        IReadOnlyCollection<string> synonyms,
        string? root = null,
        IReadOnlyCollection<string>? symbolsToIndex = null)
    {
        Id = id;
        Synonyms = synonyms;
        Root = root;
        SymbolsToIndex = symbolsToIndex;
    }
}
