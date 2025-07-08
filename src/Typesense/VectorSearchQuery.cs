using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Typesense;

/// <summary>
/// Encapsulates the vector query function utilized by Typesense.
/// Examples:
/// vec:([0.34, 0.66, 0.12, 0.68], k: 10)
/// vec:([0.34, 0.66, 0.12, 0.68], k: 10, flat_search_cutoff: 20)
/// vec:([], id: abcd)
/// </summary>
public record VectorQuery
{
    private float[] _vector = Array.Empty<float>();

    /// <summary>
    /// Document vector.
    /// </summary>
    // We only expose `ReadOnlyCollection` to make sure the consumer cannot modify the underlying value.
    public ReadOnlyCollection<float> Vector => new(_vector);

    /// <summary>
    /// Vector field name.
    /// </summary>
    public string VectorFieldName { get; private set; }

    /// <summary>
    /// Document Id.
    /// </summary>
    public string? Id { get; private set; }

    /// <summary>
    /// Number of documents to return.
    /// </summary>
    public int? K { get; private set; }
    
    /// <summary>
    /// Maximum vector distance threshold for results of semantic search and hybrid search
    /// </summary>
    public decimal? DistanceThreshold { get; private set; }

    /// <summary>
    /// Allows bypass of the HNSW index to do a flat / brute-force ranking of vectors
    /// </summary>
    public int? FlatSearchCutoff { get; private set; }

    /// <summary>
    /// Extra parameters to include in the query.
    /// </summary>
    public Dictionary<string, string> ExtraParams { get; private set; }

    /// <summary>
    /// Initialize VectorQuery using a raw query.
    /// </summary>
    /// <param name="query"></param>
    public VectorQuery(string query)
    {
        ExtraParams = new();
        VectorFieldName = "";
        ParseQuery(query);
    }

    /// <summary>
    /// Initialize VectorQuery.
    /// </summary>
    /// <param name="vector">Document vector</param>
    /// <param name="vectorFieldName">Vector field name to be searched against</param>
    /// <param name="id">String document id</param>
    /// <param name="k">Number of documents that are returned</param>
    /// <param name="flatSearchCutoff">If you wish to do brute-force vector search when a given query matches fewer than 20 documents, sending flat_search_cutoff=20 will bypass the HNSW index when the number of results found is less than 20</param>
    /// <param name="extraParams">Any extra parameters you wish to include as a key/value dictionary</param>
    /// <param name="distanceThreshold">Any extra parameters you wish to include as a key/value dictionary</param>
    /// <exception cref="ArgumentException"></exception>
    public VectorQuery(float[] vector, string vectorFieldName, string? id = null, int? k = null, int? flatSearchCutoff = null, Dictionary<string, string>? extraParams = null, decimal? distanceThreshold = null)
    {
        ArgumentNullException.ThrowIfNull(vector);

        if (vector.Length > 0 && id != null)
            throw new ArgumentException(
                "Malformed vector query string: cannot pass both vector query and `id` parameter.");

        if (vector.Length == 0 && id is null)
            throw new ArgumentException("When a vector query value is empty, an `id` parameter must be present.");

        if (string.IsNullOrWhiteSpace(vectorFieldName))
            throw new ArgumentException(
                "The vector fieldname cannot be null or whitespace.",
                nameof(vectorFieldName));

        _vector = vector;
        VectorFieldName = vectorFieldName;
        Id = id;
        K = k;
        DistanceThreshold = distanceThreshold;
        FlatSearchCutoff = flatSearchCutoff;
        ExtraParams = extraParams ?? new();
    }

    private static readonly Regex VectorQueryStringRegex = new(@"(.+):\((\[.*?\])(\s*,[^)]+)*\)", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    /// <summary>
    /// Parses a query and initializes the related object members.
    /// </summary>
    /// <param name="query"></param>
    /// <exception cref="ArgumentException"></exception>
    private void ParseQuery(string query)
    {
        // First parse the portion of the string inside the vec property - "vec:([0.96826, 0.94, 0.39557, 0.306488], k:100, flat_search_cutoff: 20)"
        var match = VectorQueryStringRegex.Match(query);
        if (!match.Success)
            throw new ArgumentException("Malformed vector query string.");

        var vectorFieldNameMatch = match.Groups[1].Value;
        if (string.IsNullOrWhiteSpace(vectorFieldNameMatch))
            throw new ArgumentException("Malformed vectory query string: it is missing the vector field name.");

        VectorFieldName = vectorFieldNameMatch;

        // Since the first parameter MUST be the array of floats this is a quick check to see if it is well-formatted
        var vectorMatch = match.Groups[2].Value;
        vectorMatch = vectorMatch.Substring(1, vectorMatch.Length - 2);

        if (!String.IsNullOrEmpty(vectorMatch))
        {
            // Get the float array query portion
            _vector = vectorMatch
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(x =>
                {
                    if (!float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                        throw new ArgumentException(
                            "Malformed vector query string: one of the vector values is not a float.");

                    return result;
                }).ToArray();
        }

        // Commas are always used as a delimiter inside the list of parameters
        var qParams = match.Groups[3].Value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var param in qParams)
        {
            var kvp = param.Split(':', StringSplitOptions.TrimEntries);

            if (kvp.Length != 2)
                throw new ArgumentException($"Malformed vector query string at parameter '{param}'");

            switch (kvp[0])
            {
                case "id":
                    if (_vector.Length > 0)
                        throw new ArgumentException(
                            "Malformed vector query string: cannot pass both vector query and `id` parameter.");

                    Id = kvp[1];
                    break;

                case "k":
                    if (!Int32.TryParse(kvp[1], out int k))
                        throw new ArgumentException("Malformed vector query string: k value is not an integer");

                    K = k;
                    break;

                case "flat_search_cutoff":
                    if (!Int32.TryParse(kvp[1], out int flatSearchCutoff))
                        throw new ArgumentException("Malformed vector query string: flat_search_cutoff value is not an integer");

                    FlatSearchCutoff = flatSearchCutoff;
                    break;

                default:
                    ExtraParams.Add(kvp[0], kvp[1]);
                    break;
            }
        }

        if (_vector.Length == 0 && Id is null)
            throw new ArgumentException("When a vector query value is empty, an `id` parameter must be present.");
    }

    /// <summary>
    /// Convert this vector query into a query useable by Typesense.
    /// </summary>
    /// <returns>The vector-search query string.</returns>
    public virtual string ToQuery()
    {
        StringBuilder queryStringBuilder = new(VectorFieldName);
        queryStringBuilder.Append(":([");
        // Float vector is required, even if empty
        for (var index = 0; index < _vector.Length; index++)
        {
            if (index != 0)
                queryStringBuilder.Append(',');
            queryStringBuilder.Append(_vector[index].ToString("R", CultureInfo.InvariantCulture));
        }
        queryStringBuilder.Append(']');

        // All other parameters are optional
        // note that the only delimiter for all type/value pairs is a comma and there is no need to surround string values with quotations
        if (Id != null)
            queryStringBuilder.Append(",id:").Append(Id);

        if (K != null)
            queryStringBuilder.Append(",k:").Append(K);

        if (FlatSearchCutoff != null)
            queryStringBuilder.Append(",flat_search_cutoff:").Append(FlatSearchCutoff);

        // Allow for additional parameters if provided
        foreach (var (key, value) in ExtraParams)
            queryStringBuilder.Append(',').Append(key).Append(':').Append(value);

        queryStringBuilder.Append(')');

        return queryStringBuilder.ToString();
    }
}
