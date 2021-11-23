using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record SynonymSchema
{
    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; init; }
    [JsonPropertyName("root")]
    public string Root { get; init; }

    public SynonymSchema(List<string> synonyms)
    {
        Synonyms = synonyms;
    }

    [JsonConstructor]
    public SynonymSchema(List<string> synonyms, string root)
    {
        Synonyms = synonyms;
        Root = root;
    }
}
