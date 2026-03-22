using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Typesense.Setup;

public record Config
{
    public IReadOnlyCollection<Node> Nodes { get; set; }
    public string ApiKey { get; set; }
    public string? SearchApiKey { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public Config()
    {
        Nodes = new List<Node>();
        ApiKey = "";
        SearchApiKey = null;
    }

    public Config(IReadOnlyCollection<Node> nodes, string apiKey, string? searchApiKey = null)
    {
        Nodes = nodes;
        ApiKey = apiKey;
        SearchApiKey = searchApiKey;
    }
}