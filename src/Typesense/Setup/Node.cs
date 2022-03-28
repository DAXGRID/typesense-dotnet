using System;

namespace Typesense.Setup;
/// <summary>
/// The node contains the configuration for a remote Typesense service.
/// </summary>
public record Node
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

    [Obsolete("Use multi-arity constructor instead.")]
    public Node()
    {
        Host = "";
        Port = "";
        Protocol = "";
    }

    /// <param name="host">Hostname for the Typesense service.</param>
    /// <param name="port">Port for the typesense service.</param>
    /// <param name="document">Protocol for the Typesense service - defaults to http.</param>
    /// <exception cref="ArgumentException"></exception>
    public Node(string host, string port, string protocol = "http")
    {
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(protocol) || string.IsNullOrEmpty(port))
            throw new ArgumentException($"{nameof(host)}, {nameof(protocol)} or {nameof(port)} cannot be null or empty");

        Host = host;
        Port = port;
        Protocol = protocol;
    }
}
