using System.Text.Json.Serialization;

namespace Typesense
{
    public class Field
    {
        [JsonPropertyName("name")]
        public string Name { get; private set; }
        [JsonPropertyName("type")]
        public string Type { get; private set; }
        [JsonPropertyName("facet")]
        public bool Facet { get; private set; }

        public Field(string name, string type, bool facet)
        {
            Name = name;
            Type = type;
            Facet = facet;
        }
    }
}
