using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense
{
    public class KeyListResponse
    {
        [JsonPropertyName("keys")]
        public IEnumerable<Key> Keys { get; init; }
    }
}