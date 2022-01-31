using System.Collections.Generic;

namespace Typesense.Setup;

public class Config
{
    public List<Node> Nodes { get; set; }
    public string ApiKey { get; set; }
}
