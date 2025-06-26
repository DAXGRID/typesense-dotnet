using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

/// <summary>
/// Converts between nullable DateTime and Unix epoch seconds as a long integer value.
/// </summary>
public class UnixEpochDateTimeLongConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        return DateTime.UnixEpoch.AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        
        writer.WriteNumberValue(Convert.ToInt64((value.Value - DateTime.UnixEpoch).TotalSeconds));
    }
}