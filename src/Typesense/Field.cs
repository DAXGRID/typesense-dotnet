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
    public bool Facet { get; init; }
    [JsonPropertyName("optional")]
    public bool? Optional { get; init; }
    [JsonPropertyName("index")]
    public bool? Index { get; init; }
    [JsonPropertyName("sort")]
    public bool? Sort { get; init; }

    public Field(string name, FieldType type)
    {
        Name = name;
        Type = type;
    }

    public Field(string name, FieldType type, bool facet)
    {
        Name = name;
        Type = type;
        Facet = facet;
    }

    public Field(string name, FieldType type, bool facet, bool? optional)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
    }

    public Field(string name, FieldType type, bool facet, bool? optional, bool? index)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
    }

    [JsonConstructor]
    public Field(string name,
                 FieldType type,
                 bool facet,
                 bool? optional,
                 bool? index,
                 bool? sort)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
        Index = index;
        Sort = sort;
    }

    [Obsolete("A better choice going forward is using the constructor with 'FieldType' enum instead.")]
    public Field(string name, string type, bool facet, bool optional = false, bool index = true)
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
            case "auto":
                return FieldType.Auto;
            case "string*":
                return FieldType.AutoString;
            default: throw new ArgumentException($"Could not map field type with value '{fieldType}'", nameof(fieldType));
        }
    }
}
