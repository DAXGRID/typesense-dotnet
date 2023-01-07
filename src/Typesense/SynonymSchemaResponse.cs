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
    public string Root { get; init; }

    [JsonConstructor]
    public SynonymSchemaResponse(string id, IReadOnlyCollection<string> synonyms, string root)
    {
        Id = id;
        Synonyms = synonyms;
        Root = root;
    }
}
