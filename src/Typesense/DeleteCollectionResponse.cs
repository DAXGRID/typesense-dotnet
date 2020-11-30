using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public record CollectionResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("num_documents")]
        public int NumberOfDocuments { get; init; }
        [JsonPropertyName("fields")]
        public IReadOnlyCollection<Field> Fields { get; init; }
        [JsonPropertyName("default_sorting_field")]
        public string DefaultSortingField { get; init; }
    }
}
