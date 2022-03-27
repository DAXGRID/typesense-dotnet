using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record ListKeysResponse
{
    [JsonPropertyName("keys")]
    public IReadOnlyCollection<KeyResponse> Keys { get; init; }

    [JsonConstructor]
    public ListKeysResponse(IReadOnlyCollection<KeyResponse> keys)
    {
        Keys = keys;
    }
}
