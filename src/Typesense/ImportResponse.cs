using System.Text.Json.Serialization;

namespace Typesense
{
    public class ImportResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
        [JsonPropertyName("document")]
        public string Document { get; set; }
    }
}
