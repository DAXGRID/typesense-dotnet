using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class GroupKeyConverter : JsonConverter<IReadOnlyList<string>>
{
    public override IReadOnlyList<string>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);

        return jsonDocument.RootElement.EnumerateArray().Select(StringifyJsonElement).ToList();
    }

    private static string StringifyJsonElement(JsonElement element)
    {
        var elementValue = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.False => "false",
            JsonValueKind.True => "true",
            JsonValueKind.Number => element.GetDecimal().ToString(CultureInfo.CreateSpecificCulture("en-US")),
            JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(StringifyJsonElement)),
            _ => null
        };

        if (elementValue is null)
            throw new InvalidOperationException($"{nameof(elementValue)} being null is invalid.");

        return elementValue;
    }
    
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<string> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}