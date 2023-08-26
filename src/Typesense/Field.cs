using System;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public record Field
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<FieldType>))]
    public FieldType Type { get; init; }

    [JsonPropertyName("facet")]
    public bool? Facet { get; init; }

    [JsonPropertyName("optional")]
    public bool? Optional { get; init; }

    [JsonPropertyName("index")]
    public bool? Index { get; init; }

    [JsonPropertyName("sort")]
    public bool? Sort { get; init; }

    [JsonPropertyName("infix")]
    public bool? Infix { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("num_dim")]
    public int? NumberOfDimensions { get; init; }

    [JsonPropertyName("embed")]
    public AutoEmbeddingConfig? Embed { get; init; }

    // This constructor is made to handle inherited classes.
    protected Field(string name)
    {
        Name = name;
    }

    public Field(string name, FieldType type)
    {
        Name = name;
        Type = type;
    }

    public Field(string name, FieldType type, int numberOfDimensions)
    {
        Name = name;
        Type = type;
        NumberOfDimensions = numberOfDimensions;
    }

    public Field(string name, FieldType type, AutoEmbeddingConfig embed)
    {
        Name = name;
        Type = type;
        Embed = embed;
    }

    public Field(string name, FieldType type, bool? facet = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
    }

    public Field(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
    }

    public Field(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
    }

    public Field(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
        Sort = sort;
    }

    public Field(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null,
        bool? infix = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
        Sort = sort;
        Infix = infix;
    }

    [JsonConstructor]
    public Field(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null,
        bool? infix = null,
        string? locale = null,
        int? numberOfDimensions = null,
        AutoEmbeddingConfig? embed = null)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
        Sort = sort;
        Infix = infix;
        Locale = locale;
        NumberOfDimensions = numberOfDimensions;
        Embed = embed;
    }

    [Obsolete("A better choice going forward is using the constructor with 'FieldType' enum instead.")]
    public Field(string name, string type, bool? facet, bool optional = false, bool index = true)
    {
        Name = name;
        Type = MapFieldType(type);
        Facet = facet;
        Optional = optional;
        Index = index;
    }

    private static FieldType MapFieldType(string fieldType)
    {
        switch (fieldType)
        {
            case "string":
                return FieldType.String;
            case "int32":
                return FieldType.Int32;
            case "int64":
                return FieldType.Int64;
            case "float":
                return FieldType.Float;
            case "bool":
                return FieldType.Bool;
            case "geopoint":
                return FieldType.GeoPoint;
            case "string[]":
                return FieldType.StringArray;
            case "int32[]":
                return FieldType.Int32Array;
            case "int64[]":
                return FieldType.Int64Array;
            case "float[]":
                return FieldType.FloatArray;
            case "bool[]":
                return FieldType.BoolArray;
            case "geopoint[]":
                return FieldType.GeoPointArray;
            case "auto":
                return FieldType.Auto;
            case "string*":
                return FieldType.AutoString;
            default: throw new ArgumentException($"Could not map field type with value '{fieldType}'", nameof(fieldType));
        }
    }
}
