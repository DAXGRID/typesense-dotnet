using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Typesense.Converter
{
    public class DateTimeOffsetConverter : JsonConverter<string>
    {
        public override string Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => reader.GetString();

        public override void Write(
            Utf8JsonWriter writer,
            string jsonValue,
            JsonSerializerOptions options) => jsonValue.ToString();
    }
}
