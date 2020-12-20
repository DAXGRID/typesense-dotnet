namespace Typesense
{
    /// <summary>
    /// The node contains the configuration for a remote Typesense service.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Hostname for the Typesense service.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Port for the Typesense service.
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// Protocol for the Typesense service - defaults to http.
        /// </summary>
        public string Protocol { get; set; } = "http";
    }
}
