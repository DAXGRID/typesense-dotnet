using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

/// <summary>
/// Converts between nullable DateTime and Unix epoch seconds as a long integer value.
/// </summary>
public class UnixEpochDateTimeLongConverter : JsonConverter<DateTime?>
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return UnixEpoch.AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        Guard.ThrowIfNull(writer, nameof(writer));

        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(Convert.ToInt64((value.Value - UnixEpoch).TotalSeconds));
    }
}
