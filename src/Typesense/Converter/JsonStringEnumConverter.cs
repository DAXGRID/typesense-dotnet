using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class JsonStringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, System.Enum
{
    private readonly Dictionary<TEnum, string> _enumToString = new Dictionary<TEnum, string>();
    private readonly Dictionary<string, TEnum> _stringToEnum = new Dictionary<string, TEnum>();

    public JsonStringEnumConverter()
    {
        var type = typeof(TEnum);
        var values = System.Enum.GetValues<TEnum>();

        foreach (var value in values)
        {
            var enumMember = type.GetMember(value.ToString())[0];
            var attr = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false)
              .Cast<EnumMemberAttribute>()
              .FirstOrDefault();

            _stringToEnum.Add(value.ToString(), value);

            if (attr?.Value != null)
            {
                _enumToString.Add(value, attr.Value);
                _stringToEnum.Add(attr.Value, value);
            }
            else
            {
                _enumToString.Add(value, value.ToString());
            }
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (stringValue is null)
            throw new InvalidOperationException($"Received null value from {nameof(reader)}.");

        if (_stringToEnum.TryGetValue(stringValue, out var enumValue))
            return enumValue;

        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        writer.WriteStringValue(_enumToString[value]);
    }
}
