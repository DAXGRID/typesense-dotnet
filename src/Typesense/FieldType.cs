using System.Runtime.Serialization;

namespace Typesense;

public enum FieldType
{
    [EnumMember(Value = "string")]
    String,
    [EnumMember(Value = "int32")]
    Int32,
    [EnumMember(Value = "int64")]
    Int64,
    [EnumMember(Value = "float")]
    Float,
    [EnumMember(Value = "bool")]
    Bool,
    [EnumMember(Value = "geopoint")]
    GeoPoint,
    [EnumMember(Value = "string[]")]
    StringArray,
    [EnumMember(Value = "int32[]")]
    Int32Array,
    [EnumMember(Value = "int64[]")]
    Int64Array,
    [EnumMember(Value = "float[]")]
    FloatArray,
    [EnumMember(Value = "bool[]")]
    BoolArray,
    [EnumMember(Value = "geopoint[]")]
    GeoPointArray,
    [EnumMember(Value = "auto")]
    Auto,
    [EnumMember(Value = "string*")]
    AutoString,
}
