using System.Collections.Generic;

namespace Typesense
{
    public class Config
    {
        public List<Node> Nodes { get; set; }
        public string ApiKey { get; set; }
        public int ConnectionTimeoutSeconds { get; set; }
    }
}
