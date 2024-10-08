using System.Runtime.Serialization;

namespace Typesense;

public enum FieldType
{
    [EnumMember(Value = "string")]
    String,

    [EnumMember(Value = "string[]")]
    StringArray,

    [EnumMember(Value = "int32")]
    Int32,

    [EnumMember(Value = "int32[]")]
    Int32Array,

    [EnumMember(Value = "int64")]
    Int64,

    [EnumMember(Value = "int64[]")]
    Int64Array,

    [EnumMember(Value = "float")]
    Float,

    [EnumMember(Value = "float[]")]
    FloatArray,

    [EnumMember(Value = "bool")]
    Bool,

    [EnumMember(Value = "bool[]")]
    BoolArray,

    [EnumMember(Value = "geopoint")]
    GeoPoint,

    [EnumMember(Value = "geopoint[]")]
    GeoPointArray,

    [EnumMember(Value = "object")]
    Object,

    [EnumMember(Value = "object[]")]
    ObjectArray,
    
    [EnumMember(Value = "image")]
    Image,

    [EnumMember(Value = "string*")]
    AutoString,

    [EnumMember(Value = "auto")]
    Auto,
}
