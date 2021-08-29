using System.Text.Json.Serialization;

namespace Typesense
{
    public record DeleteKeyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
