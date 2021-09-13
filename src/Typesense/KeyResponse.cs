using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public class KeyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("value_prefix")]
        public string ValuePrefix { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("actions")]
        public IEnumerable<string> Actions { get; set; }
        [JsonPropertyName("collections")]
        public IEnumerable<string> Collections { get; set; }
        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }
}
