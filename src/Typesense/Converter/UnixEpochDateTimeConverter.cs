using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter;

public class UnixEpochDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return UnixEpoch.AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        Guard.ThrowIfNull(writer, nameof(writer));
        writer.WriteStringValue((value - UnixEpoch).TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}
