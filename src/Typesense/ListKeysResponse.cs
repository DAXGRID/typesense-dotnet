using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public class ListKeysResponse
{
    [JsonPropertyName("keys")]
    public IEnumerable<KeyResponse> Keys { get; init; }
}
