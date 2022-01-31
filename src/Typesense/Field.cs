using System;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public class Field
{
    [JsonPropertyName("name")]
    public string Name { get; private set; }
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<FieldType>))]
    public FieldType Type { get; private set; }
    [JsonPropertyName("facet")]
    public bool Facet { get; private set; }
    [JsonPropertyName("optional")]
    public bool Optional { get; private set; }

    [JsonConstructor]
    public Field(string name, FieldType type, bool facet, bool optional = false)
    {
        Name = name;
        Type = type;
        Facet = facet;
        Optional = optional;
    }

    [Obsolete("A better choice going forward is using the constructor with 'FieldType' enum instead.")]
    public Field(string name, string type, bool facet, bool optional = false)
    {
        Name = name;
        Type = MapFieldType(type);
        Facet = facet;
        Optional = optional;
    }

    private FieldType MapFieldType(string fieldType)
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
            default: throw new Exception($"Could not map field type {nameof(fieldType)}");
        }
    }
}
