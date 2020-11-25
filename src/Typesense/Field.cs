using System.Text.Json.Serialization;

namespace Typesense
{
    public class Field
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("facet")]
        public bool Facet { get; set; }
    }
}
