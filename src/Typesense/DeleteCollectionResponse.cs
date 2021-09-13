using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public class CollectionResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("num_documents")]
        public int NumberOfDocuments { get; set; }
        [JsonPropertyName("fields")]
        public IReadOnlyCollection<Field> Fields { get; set; }
        [JsonPropertyName("default_sorting_field")]
        public string DefaultSortingField { get; set; }
    }
}
