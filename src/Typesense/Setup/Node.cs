using System;
using System.Linq;

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
    /// <summary>
    /// Protocol for the Typesense service - defaults to http.
    /// </summary>
    public string AdditionalPath { get; set; } = "";

    [Obsolete("Use multi-arity constructor instead.")]
    public Node()
    {
        Host = "";
        Port = "";
        Protocol = "";
    }

    /// <param name="host">Hostname for the Typesense service.</param>
    /// <param name="port">Port for the typesense service.</param>
    /// <param name="protocol">Protocol for the Typesense service - defaults to http.</param>
    /// <exception cref="ArgumentException"></exception>
    public Node(string host, string port, string protocol = "http")
    {
        if (string.IsNullOrEmpty(host))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(host));
        }

        if (string.IsNullOrEmpty(protocol))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(protocol));
        }

        if (string.IsNullOrEmpty(port))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(port));
        }

        Host = host;
        Port = port;
        Protocol = protocol;
    }

    /// <param name="host">Hostname for the Typesense service.</param>
    /// <param name="port">Port for the typesense service.</param>
    /// <param name="protocol">Protocol for the Typesense service - defaults to http.</param>
    /// <param name="additionalPath">additionalPath (without trailing or leading '/') for the Typesense service - defaults to empty string.</param>
    /// <exception cref="ArgumentException"></exception>
    public Node(string host, string port, string protocol = "http", string additionalPath = "")
    {
        if (string.IsNullOrEmpty(host))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(host));
        }

        if (string.IsNullOrEmpty(protocol))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(protocol));
        }

        if (string.IsNullOrEmpty(port))
        {
            throw new ArgumentException("Cannot be NULL or empty.", nameof(port));
        }

        Host = host;
        Port = port;
        Protocol = protocol;
        AdditionalPath = additionalPath + "/";
    }
}
