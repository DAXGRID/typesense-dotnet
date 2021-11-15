using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;
public record KeyResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("value")]
    public string Value { get; init; }
    [JsonPropertyName("value_prefix")]
    public string ValuePrefix { get; init; }
    [JsonPropertyName("description")]
    public string Description { get; init; }
    [JsonPropertyName("actions")]
    public IEnumerable<string> Actions { get; init; }
    [JsonPropertyName("collections")]
    public IEnumerable<string> Collections { get; init; }
    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; init; }
}
