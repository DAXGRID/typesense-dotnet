using System;
using System.Collections.Generic;

namespace Typesense.Setup;

public record Config
{
    public IReadOnlyCollection<Node> Nodes { get; set; }
    public string ApiKey { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public Config()
    {
        Nodes = new List<Node>();
        ApiKey = "";
    }

    public Config(IReadOnlyCollection<Node> nodes, string apiKey)
    {
        Nodes = nodes;
        ApiKey = apiKey;
    }
}
