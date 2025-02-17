using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace Typesense.Converter;

public class UnixEpochDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.UnixEpoch.AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue((value - DateTime.UnixEpoch).TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}
