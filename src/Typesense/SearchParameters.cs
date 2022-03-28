using System;

namespace Typesense;

public record SearchParameters
{
    /// <summary>
    /// The query text to search for in the collection.
    /// Use * as the search string to return all documents.
    /// This is typically useful when used in conjunction with filter_by.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// A list of `string` fields that should be queried against. Multiple fields are separated with a comma.
    /// </summary>
    public string QueryBy { get; set; }

    /// <summary>
    /// The relative weight to give each `query_by` field when ranking results.
    /// This can be used to boost fields in priority, when looking for matches.
    /// Multiple fields are separated with a comma.
    /// </summary>
    public string? QueryByWeights { get; set; }

    /// <summary>
    /// Maximum number of hits returned. Increasing this value might
    /// increase search latency. Default: 500. Use `all` to return all hits found.
    /// </summary>
    [Obsolete("max_hits has been deprecated since Typesense version 0.19.0")]
    public string? MaxHits { get; set; }

    /// <summary>
    /// Boolean field to indicate that the last word in the query should
    /// be treated as a prefix, and not as a whole word. This is used for building
    /// autocomplete and instant search interfaces. Defaults to true.
    /// </summary>
    public bool? Prefix { get; set; }

    /// <summary>
    /// Filter conditions for refining your search results. Separate
    /// multiple conditions with &&.
    /// </summary>
    public string? FilterBy { get; set; }

    /// <summary>
    /// A list of numerical fields and their corresponding sort orders
    /// that will be used for ordering your results.
    /// Up to 3 sort fields can be specified.
    /// The text similarity score is exposed as a special `_text_match` field that
    /// you can use in the list of sorting fields.
    /// If no `sort_by` parameter is specified, results are sorted by
    /// `_text_match:desc,default_sorting_field:desc`
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// A list of fields that will be used for faceting your results
    /// on. Separate multiple fields with a comma.
    /// </summary>
    public string? FacetBy { get; set; }

    /// <summary>
    /// Maximum number of facet values to be returned.
    /// </summary>
    public string? MaxFacetValues { get; set; }

    /// <summary>
    /// Facet values that are returned can now be filtered via this parameter.
    /// The matching facet text is also highlighted. For example, when faceting
    /// by `category`, you can set `facet_query=category:shoe` to return only
    /// facet values that contain the prefix "shoe".
    /// </summary>
    public string? FacetQuery { get; set; }

    /// <summary>
    /// The number of typographical errors (1 or 2) that would be tolerated.
    /// </summary>
    public string? NumberOfTypos { get; set; }

    /// <summary>
    /// Results from this specific page number would be fetched.
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// Number of results to fetch per page. Default: 10
    /// </summary>
    public string? PerPage { get; set; }

    /// <summary>
    /// You can aggregate search results into groups or buckets by specify
    /// one or more `group_by` fields. Separate multiple fields with a comma.
    /// To group on a particular field, it must be a faceted field.
    /// </summary>
    public string? GroupBy { get; set; }

    /// <summary>
    /// Maximum number of hits to be returned for every group. If the `group_limit` is
    /// set as `K` then only the top K hits in each group are returned in the response.
    /// </summary>
    public string? GroupLimit { get; set; }

    /// <summary>
    /// List of fields from the document to include in the search result.
    /// </summary>
    public string? IncludeFields { get; set; }

    /// <summary>
    /// List of fields from the document to exclude in the search result.
    /// </summary>
    public string? ExcludeFields { get; set; }

    /// <summary>
    /// List of fields which should be highlighted fully without snippeting.
    /// </summary>
    public string? HighlightFullFields { get; set; }

    /// <summary>
    /// The number of tokens that should surround the highlighted text on each side.
    /// </summary>
    public string? HighlightAffixNumberOfTokens { get; set; }

    /// <summary>
    /// The start tag used for the highlighted snippets.
    /// </summary>
    public string? HighlightStartTag { get; set; }

    /// <summary>
    /// The end tag used for the highlighted snippets.
    /// </summary>
    public string? HighlightEndTag { get; set; }

    /// <summary>
    /// Field values under this length will be fully highlighted, instead of showing
    /// a snippet of relevant portion. Default: 30
    /// </summary>
    public string? SnippetThreshold { get; set; }

    /// <summary>
    /// If the number of results found for a specific query is less than
    /// this number, Typesense will attempt to drop the tokens in the query until
    /// enough results are found. Tokens that have the least individual hits
    /// are dropped first. Set to 0 to disable. Default: 10
    /// </summary>
    public string? DropTokensThreshold { get; set; }

    /// <summary>
    /// If the number of results found for a specific query is less than this number,
    /// Typesense will attempt to look for tokens with more typos until
    /// enough results are found. Default: 100
    /// </summary>
    public string? TypoTokensThreshold { get; set; }

    /// <summary>
    /// A list of records to unconditionally include in the search results
    /// at specific positions. An example use case would be to feature or promote
    /// certain items on the top of search results.
    /// A list of `record_id:hit_position`. Eg: to include a record with ID 123
    /// at Position 1 and another record with ID 456 at Position 5,
    /// you'd specify `123:1,456:5`.
    /// You could also use the Overrides feature to override search results based
    /// on rules. Overrides are applied first, followed by `pinned_hits` and
    /// finally `hidden_hits`.
    /// </summary>
    public string? PinnedHits { get; set; }

    /// <summary>
    /// A list of records to unconditionally hide from search results.
    /// A list of `record_id`s to hide. Eg: to hide records with IDs 123 and 456,
    /// you'd specify `123,456`.
    /// You could also use the Overrides feature to override search results based
    /// on rules. Overrides are applied first, followed by `pinned_hits` and
    /// finally `hidden_hits`.
    /// </summary>
    public string? HiddenHits { get; set; }

    /// <summary>
    /// Maximum number of hits that can be fetched from the collection. Eg: 200
    /// page * per_page should be less than this number for the search request to return results.
    /// A list of custom fields that must be highlighted even if you don't query
    /// for them.
    /// </summary>
    public string? LimitHits { get; set; }

    /// <summary>
    /// Set this parameter to true if you wish to split the search query into space separated words yourself.
    /// When set to true, we will only split the search query by space,
    /// instead of using the locale-aware, built-in tokenizer.
    /// </summary>
    public bool? PreSegmentedQuery { get; set; }

    /// <summary>
    /// If you have some overrides defined but want to disable all of them during
    /// query time, you can do that by setting this parameter to false
    /// </summary>
    public bool? EnableOverrides { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public SearchParameters()
    {
        Text = string.Empty;
        QueryBy = string.Empty;
    }

    public SearchParameters(string text, string queryBy)
    {
        Text = text;
        QueryBy = queryBy;
    }
}
