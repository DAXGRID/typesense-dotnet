using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class VectorQueryJsonConverter : JsonConverter<VectorQuery>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(VectorQuery);
    }

    public override VectorQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonValue = reader.GetString() ?? string.Empty;
        return !String.IsNullOrEmpty(jsonValue) ? new VectorQuery(jsonValue) : null!;
    }

    public override void Write(Utf8JsonWriter writer, VectorQuery value, JsonSerializerOptions options)
    {
        Guard.ThrowIfNull(writer, nameof(writer));
        Guard.ThrowIfNull(value, nameof(value));

        writer.WriteStringValue(value.ToQuery());
    }
}
