using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class MatchedTokenConverter : JsonConverter<IReadOnlyList<object>>
{
    public override IReadOnlyList<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var matchedTokens = new List<object>();

        foreach (var element in jsonDocument.RootElement.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var elementValue = element.GetString();
                if (elementValue is null)
                    throw new InvalidOperationException($"{nameof(elementValue)} being null is invalid.");

                matchedTokens.Add(elementValue);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var elements = new List<string>();
                foreach (var stringElement in element.EnumerateArray())
                {
                    var elementValue = stringElement.GetString();
                    if (elementValue is null)
                        throw new InvalidOperationException($"{nameof(elementValue)} being null is invalid.");

                    elements.Add(elementValue);
                }

                matchedTokens.Add(elements);
            }
        }

        return matchedTokens;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<object> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
