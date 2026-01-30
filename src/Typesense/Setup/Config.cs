using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Typesense.Setup;

public record Config
{
    private static readonly Version V30 = new(30, 0);

    public IReadOnlyCollection<Node> Nodes { get; set; }
    public string ApiKey { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// The minimum version of the Typesense server that this client is compatible with.
    /// By default, this is <c>null</c>, meaning that the client is compatible with recent versions before v30.
    /// </summary>
    /// <remarks>
    /// This property can be used to e.g. handle the synonym breaking changes that were introduced in Typesense v30.
    /// See https://typesense.org/docs/30.0/api/#deprecations-behavior-changes
    /// </remarks>
    public Version? MinimumCompatibilityVersion { get; set; }

    public bool AreSynonymSetsSupported => MinimumCompatibilityVersion >= V30;
    public bool AreSynonymsSupported => !AreSynonymSetsSupported;

    public bool AreCurationSetsSupported => MinimumCompatibilityVersion >= V30;
    public bool AreOverridesSupported => !AreCurationSetsSupported;

    [Obsolete("Use multi-arity constructor instead.")]
    public Config()
    {
        Nodes = new List<Node>();
        ApiKey = "";
    }

    public Config(IReadOnlyCollection<Node> nodes, string apiKey, Version? minimumCompatibilityVersion = null)
    {
        Nodes = nodes;
        ApiKey = apiKey;
        MinimumCompatibilityVersion = minimumCompatibilityVersion;
    }

    public void ThrowIfSynonymSetsAreNotSupported()
    {
        if (!AreSynonymSetsSupported)
            throw new NotSupportedException("Synonym sets are not supported by the current configured Typesense server version. " +
                "Use synonymns instead or change the configuration.");
    }

    public void ThrowIfSynonymsAreNotSupported()
    {
        if (!AreSynonymsSupported)
            throw new NotSupportedException("Synonyms are not supported by the current configured Typesense server version. "
                +" Use synonym sets instead or change the configuration.");
    }

    public void ThrowIfCurationSetsAreNotSupported()
    {
        if (!AreCurationSetsSupported)
            throw new NotSupportedException("Curation sets are not supported by the current configured Typesense server version. " +
                "Use overrides instead or change the configuration.");
    }

    public void ThrowIfOverridesAreNotSupported()
    {
        if (!AreOverridesSupported)
            throw new NotSupportedException("Overrides are not supported by the current configured Typesense server version. " +
                "Use curation sets instead or change the configuration.");
    }
}