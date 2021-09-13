using System.Text.Json.Serialization;

namespace Typesense
{
    public class FilterDeleteResponse
    {
        [JsonPropertyName("num_deleted")]
        public int NumberOfDeleted { get; set; }
    }
}
