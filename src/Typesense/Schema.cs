using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record Schema
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("fields")]
    public IEnumerable<Field> Fields { get; init; }

    [JsonPropertyName("default_sorting_field")]
    public string? DefaultSortingField { get; init; }

    [JsonPropertyName("token_separators")]
    public IEnumerable<string>? TokenSeparators { get; init; }

    [JsonPropertyName("symbols_to_index")]
    public IEnumerable<string>? SymbolsToIndex { get; init; }

    [JsonPropertyName("enable_nested_fields")]
    public bool? EnableNestedFields { get; init; }

    [JsonPropertyName("metadata")]
    public IDictionary<string, object?>? Metadata { get; init; }

    [Obsolete("Use multi-arity constructor instead.")]
    public Schema()
    {
        Name = "";
        Fields = new List<Field>();
    }

    public Schema(string name, IEnumerable<Field> fields)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        Fields = fields;
    }

    public Schema(string name, IEnumerable<Field> fields, string defaultSortingField)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(name);

        Name = name;
        Fields = fields;
        DefaultSortingField = defaultSortingField;
    }
}
