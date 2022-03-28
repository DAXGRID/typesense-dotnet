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
    public string? Description { get; init; }
    [JsonPropertyName("actions")]
    public IReadOnlyCollection<string>? Actions { get; init; }
    [JsonPropertyName("collections")]
    public IReadOnlyCollection<string>? Collections { get; init; }
    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; init; }

    [JsonConstructor]
    public KeyResponse(
        int id,
        string value,
        string valuePrefix,
        long expiresAt,
        string? description = null,
        IReadOnlyCollection<string>? actions = null,
        IReadOnlyCollection<string>? collections = null)
    {
        Id = id;
        Value = value;
        ValuePrefix = valuePrefix;
        ExpiresAt = expiresAt;
        Description = description;
        Actions = actions;
        Collections = collections;
    }
}
