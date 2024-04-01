using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record Key
{
    [JsonPropertyName("actions")]
    public IReadOnlyCollection<string> Actions { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("collections")]
    public IReadOnlyCollection<string> Collections { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; init; }

    [JsonPropertyName("autodelete")]
    public bool? AutoDelete { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public Key()
    {
        Actions = new List<string>();
        Description = string.Empty;
        Collections = new List<string>();
    }

    public Key(
        string description,
        IReadOnlyCollection<string> actions,
        IReadOnlyCollection<string> collections)
    {
        Actions = actions;
        Description = description;
        Collections = collections;
    }
}
