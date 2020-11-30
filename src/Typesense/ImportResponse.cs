using System.Text.Json.Serialization;

namespace Typesense
{
    public record ImportResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        [JsonPropertyName("error")]
        public string Error { get; init; }
        [JsonPropertyName("document")]
        public string Document { get; init; }
    }
}
