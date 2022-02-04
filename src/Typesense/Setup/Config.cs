using System.Collections.Generic;

namespace Typesense.Setup;

public record Config
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage
    ("Usage", "CA2227: Collection properties should be read only", Justification = "Do not want to break existing consumers.")]
    public IEnumerable<Node> Nodes { get; set; }
    public string ApiKey { get; set; }
}
