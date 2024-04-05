using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class JsonStringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    // These can be made FrozenDictionary when .NET 6 & 7 support is removed 
    private static readonly Dictionary<TEnum, string> EnumToString;
    private static readonly Dictionary<string, TEnum> StringToEnum = new();

    static JsonStringEnumConverter()
    {
        var type = typeof(TEnum);
        var values = Enum.GetValues<TEnum>();
        EnumToString = new Dictionary<TEnum, string>(capacity: values.Length);

        foreach (var value in values)
        {
            var stringValue = value.ToString();
            var enumMember = type.GetMember(stringValue)[0];
            var attr = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false)
              .Cast<EnumMemberAttribute>()
              .FirstOrDefault();

            StringToEnum.Add(stringValue, value);

            if (attr?.Value != null)
            {
                EnumToString.Add(value, attr.Value);
                StringToEnum.Add(attr.Value, value);
            }
            else
            {
                EnumToString.Add(value, stringValue);
            }
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (stringValue is null)
            throw new InvalidOperationException($"Received null value from {nameof(reader)}.");

        return StringToEnum.GetValueOrDefault(stringValue);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(EnumToString[value]);
    }
}
