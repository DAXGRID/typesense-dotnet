using System.Text.Json.Serialization;

namespace Typesense
{
    public class DeleteKeyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
