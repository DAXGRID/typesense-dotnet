using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public record CollectionResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("num_documents")]
    public int NumberOfDocuments { get; init; }

    [JsonPropertyName("fields")]
    public IReadOnlyCollection<Field> Fields { get; init; }

    [JsonPropertyName("default_sorting_field")]
    public string DefaultSortingField { get; init; }

    [JsonPropertyName("token_separators")]
    public IReadOnlyCollection<string> TokenSeparators { get; init; }

    [JsonPropertyName("symbols_to_index")]
    public IReadOnlyCollection<string> SymbolsToIndex { get; init; }

    [JsonPropertyName("enable_nested_fields")]
    public bool EnableNestedFields { get; init; }

    [JsonPropertyName("metadata")]
    public IDictionary<string, object>? Metadata { get; init; }

    [JsonPropertyName("created_at"), JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime CreatedAt { get; init; }

    [JsonConstructor]
    public CollectionResponse(
        string name,
        int numberOfDocuments,
        IReadOnlyCollection<Field> fields,
        string defaultSortingField,
        IReadOnlyCollection<string> tokenSeparators,
        IReadOnlyCollection<string> symbolsToIndex,
        bool enableNestedFields,
        IDictionary<string, object>? metadata = null)
    {
        Name = name;
        NumberOfDocuments = numberOfDocuments;
        Fields = fields;
        DefaultSortingField = defaultSortingField;
        TokenSeparators = tokenSeparators;
        SymbolsToIndex = symbolsToIndex;
        EnableNestedFields = enableNestedFields;
        Metadata = metadata;
    }
}
