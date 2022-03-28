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

    [Obsolete("Use multi-arity constructor instead.")]
    public Schema()
    {
        Name = "";
        Fields = new List<Field>();
    }

    public Schema(string name, IEnumerable<Field> fields)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException();
        Name = name;
        Fields = fields;
    }

    public Schema(string name, IEnumerable<Field> fields, string defaultSortingField)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException();
        Name = name;
        Fields = fields;
        DefaultSortingField = defaultSortingField;
    }
}
