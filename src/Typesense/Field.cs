using System.Text.Json.Serialization;

namespace Typesense
{
    public class Field
    {
        [JsonPropertyName("name")]
        public string Name { get; private set; }
        [JsonPropertyName("type")]
        public FieldType Type { get; private set; }
        [JsonPropertyName("facet")]
        public bool Facet { get; private set; }
        [JsonPropertyName("optional")]
        public bool Optional { get; private set; }

        public Field(string name, FieldType type, bool facet, bool optional = false)
        {
            Name = name;
            Type = type;
            Facet = facet;
            Optional = optional;
        }
    }
}
