using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public enum SplitJoinTokenOption
{
    Fallback,
    Always,
    Off
}

public record MultiSearchParameters : SearchParameters
{
    /// <summary>
    /// The collection to query.
    /// </summary>
    [JsonPropertyName("collection")]
    public string Collection { get; set; }

    // ---------------------------------------------------------------------------------------
    // Vector Search - https://typesense.org/docs/0.24.1/api/vector-search.html#what-is-an-embedding
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Query-string for vector searches.
    /// </summary>
    [JsonConverter(typeof(VectorQueryJsonConverter)), JsonPropertyName("vector_query")]
    public VectorQuery? VectorQuery { get; init; }

    [JsonPropertyName("group_by")]
    public string GroupBy { get; set; }

    [JsonPropertyName("group_limit")]
    public int? GroupLimit { get; set; }

    [JsonPropertyName("group_missing_values")]
    public bool? GroupMissingValues { get; set; }

    /// <summary>
    /// Set this parameter to the value of a preset that has been created in typesense.
    /// The query parameters of the preset will then be used in your search.
    /// </summary>
    [JsonPropertyName("preset")]
    public string? Preset { get; set; }

    public MultiSearchParameters(string collection, string text) : base(text)
    {
        Collection = collection;
    }

    public MultiSearchParameters(string collection, string text, string queryBy) : base(text, queryBy)
    {
        Collection = collection;
    }
}

public record SearchParameters
{
    // -------------------------------------------------------------------------------------
    // Query parameters - https://typesense.org/docs/latest/api/search.html#query-parameters
    // -------------------------------------------------------------------------------------

    /// <summary>
    /// The query text to search for in the collection.
    /// Use * as the search string to return all documents.
    /// This is typically useful when used in conjunction with filter_by.
    /// </summary>
    [JsonPropertyName("q")]
    public string Text { get; set; }

    /// <summary>
    /// A list of `string` fields that should be queried against. Multiple fields are separated with a comma.
    /// </summary>
    [JsonPropertyName("query_by")]
    public string? QueryBy { get; set; }

    /// <summary>
    /// Boolean field to indicate that the last word in the query should
    /// be treated as a prefix, and not as a whole word. This is used for building
    /// autocomplete and instant search interfaces. Defaults to true.
    /// </summary>
    /// <remarks>Backwards compatible implementation for existing clients.</remarks>
    public bool? Prefix
    {
        get => PrefixList == null ? null : PrefixList.Count > 0 ? PrefixList[0] : null;
        set
        {
            if (value != null) 
            {
                if (PrefixList == null)
                {
                    PrefixList = new List<bool>();
                }
                if(PrefixList.Count == 0)
                {
                    PrefixList.Add(value.Value);
                }
                else
                {
                    PrefixList[0] = value.Value!;
                }
            }
        }
    }

    /// <summary>
    /// List of boolean fields to indicate that the last word in the query should
    /// be treated as a prefix, and not as a whole word. This is used for building
    /// autocomplete and instant search interfaces. 
    /// For example, if you are querying 3 fields and want to enable prefix searching only on the first field
    /// you can set the prefix property to [true, false, false].
    /// Defaults to true.
    /// </summary>
    [JsonPropertyName("prefix")]
    public List<bool> PrefixList { get; set; }

    /// <summary>
    /// If infix index is enabled for this field, infix searching can be done on a per-field basis by sending a comma separated string parameter called infix to the search query.
    /// This parameter can have 3 values:
    /// off: infix search is disabled, which is default
    /// always: infix search is performed along with regular search
    /// fallback: infix search is performed if regular search does not produce results
    /// </summary>
    [JsonPropertyName("infix")]
    public string? Infix { get; set; }

    /// <summary>
    /// Set this parameter to the value of a preset that has been created in typesense.
    /// The query parameters of the preset will then be used in your search.
    /// </summary>
    [JsonPropertyName("preset")]
    public string? Preset { get; set; }

    /// <summary>
    /// Set this parameter to true if you wish to split the search query into space separated words yourself.
    /// When set to true, we will only split the search query by space,
    /// instead of using the locale-aware, built-in tokenizer.
    /// </summary>
    [JsonPropertyName("pre_segmented_query")]
    public bool? PreSegmentedQuery { get; set; }

    /// <summary>
    /// A comma separated list of words to be dropped from the search query while searching.
    /// </summary>
    [JsonPropertyName("stopwords")]
    public string? Stopwords { get; set; }

    /// <summary>
    /// Controls whether Typesense should validate if the fields exist in the schema. 
    /// When set to false, Typesense will not throw an error if a field is missing.
    /// This is useful for programmatic grouping where not all fields may exist.
    /// </summary>
    [JsonPropertyName("validate_field_names")]
    public bool? ValidateFieldNames { get; set; }

    // ---------------------------------------------------------------------------------------
    // Filter parameters - https://typesense.org/docs/latest/api/search.html#filter-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Filter conditions for refining your search results.
    /// Separate multiple conditions with &amp;&amp;.
    /// </summary>
    [JsonPropertyName("filter_by")]
    public string? FilterBy { get; set; }

    /// <summary>
    /// Applies the filtering operation incrementally / lazily. 
    /// Set this to true when you are potentially filtering on large values but the tokens in the query are expected to match very few documents. 
    /// Default: false
    /// </summary>
    [JsonPropertyName("enable_lazy_filter")]
    public bool? EnableLazyFilter { get; set; }

    /// <summary>
    /// Controls the number of similar words that Typesense considers during fuzzy search on filter_by values. 
    /// Useful for controlling prefix matches like company_name:Acm*. 
    /// Default: 4
    /// </summary>
    [JsonPropertyName("max_filter_by_candidates")]
    public int? MaxFilterByCandidates { get; set; }

    // ---------------------------------------------------------------------------------------
    // Ranking and Sorting parameters - https://typesense.org/docs/latest/api/search.html#ranking-and-sorting-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// The relative weight to give each `query_by` field when ranking results.
    /// This can be used to boost fields in priority, when looking for matches.
    /// Multiple fields are separated with a comma.
    /// </summary>
    [JsonPropertyName("query_by_weights")]
    public string? QueryByWeights { get; set; }

    /// <summary>
    /// In a multi-field matching context, this parameter determines how the representative text match score of a record is calculated.
    /// This parameter can have 2 values:
    /// max_score: the best text match score across all fields are used as the representative score of this record. Field weights are used as tie breakers when 2 records share the same text match score.
    /// max_weight: the text match score of the highest weighted field is used as the representative text relevancy score of the record.
    /// </summary>
    [JsonPropertyName("text_match_type")]
    public string? TextMatchType { get; set; }

    /// <summary>
    /// A list of numerical fields and their corresponding sort orders
    /// that will be used for ordering your results.
    /// Up to 3 sort fields can be specified.
    /// The text similarity score is exposed as a special `_text_match` field that
    /// you can use in the list of sorting fields.
    /// If no `sort_by` parameter is specified, results are sorted by
    /// `_text_match:desc,default_sorting_field:desc`
    /// </summary>
    [JsonPropertyName("sort_by")]
    public string? SortBy { get; set; }

    /// <summary>
    /// By default, Typesense prioritizes documents whose field value matches
    /// exactly with the query. Set this parameter to `false` to disable this behavior.
    /// Defaults to true.
    /// </summary>
    [JsonPropertyName("prioritize_exact_match")]
    public bool? PrioritizeExactMatch { get; set; }

    /// <summary>
    /// Make Typesense prioritize documents where the query words appear earlier in the text.
    /// </summary>
    [JsonPropertyName("prioritize_token_position")]
    public bool? PrioritizeTokenPosition { get; set; }

    /// <summary>
    /// Make Typesense prioritize documents where the query words appear in more number of fields.
    /// Default: true
    /// </summary>
    [JsonPropertyName("prioritize_num_matching_fields")]
    public bool? PrioritizeNumberMatchingFields { get; set; }

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
    [JsonPropertyName("pinned_hits")]
    public string? PinnedHits { get; set; }

    /// <summary>
    /// A list of records to unconditionally hide from search results.
    /// A list of `record_id`s to hide. Eg: to hide records with IDs 123 and 456,
    /// you'd specify `123,456`.
    /// You could also use the Overrides feature to override search results based
    /// on rules. Overrides are applied first, followed by `pinned_hits` and
    /// finally `hidden_hits`.
    /// </summary>
    [JsonPropertyName("hidden_hits")]
    public string? HiddenHits { get; set; }

    /// <summary>
    /// Whether the filter_by condition of the search query should be applicable to curated results (override definitions, pinned hits, hidden hits, etc.).
    /// </summary>
    [JsonPropertyName("filter_curated_hits")]
    public bool? FilterCuratedHits { get; set; }

    /// <summary>
    /// If you have some overrides defined but want to disable all of them during
    /// query time, you can do that by setting this parameter to false
    /// </summary>
    [JsonPropertyName("enable_overrides")]
    public bool? EnableOverrides { get; set; }

    /// <summary>
    /// You can trigger particular override rules that you've tagged using their tag name(s) in this search parameter.
    /// </summary>
    [JsonPropertyName("override_tags")]
    public string? OverrideTags { get; set; }

    /// <summary>
    /// If you have some synonyms defined but want to disable all of them for a particular search query, set enable_synonyms to false.
    /// Default: true
    /// </summary>
    [JsonPropertyName("enable_synonyms")]
    public bool? EnableSynonyms { get; set; }

    /// <summary>
    /// Allow synonym resolution on word prefixes in the query.
    /// Default: false
    /// </summary>
    [JsonPropertyName("synonym_prefix")]
    public bool? SynonymPrefix { get; set; }

    // ---------------------------------------------------------------------------------------
    // Pagination parameters - https://typesense.org/docs/latest/api/search.html#pagination-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Results from this specific page number would be fetched.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Number of results to fetch per page. Default: 10
    /// </summary>
    [JsonPropertyName("per_page")]
    public int? PerPage { get; set; }

    /// <summary>
    /// Identifies the starting point to return hits from a result set. Can be used as an alternative to the page parameter.
    /// </summary>
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    /// <summary>
    /// Number of hits to fetch. Can be used as an alternative to the per_page parameter. Default: 10.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Maximum number of hits returned. Increasing this value might
    /// increase search latency. Default: 500. Use `all` to return all hits found.
    /// </summary>
    [Obsolete("max_hits has been deprecated since Typesense version 0.19.0")]
    [JsonPropertyName("max_hits")]
    public int? MaxHits { get; set; }

    // ---------------------------------------------------------------------------------------
    // Faceting parameters - https://typesense.org/docs/latest/api/search.html#faceting-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// A list of fields that will be used for faceting your results
    /// on. Separate multiple fields with a comma.
    /// </summary>
    [JsonPropertyName("facet_by")]
    public string? FacetBy { get; set; }

    /// <summary>
    /// Typesense supports two strategies for efficient faceting, and has some built-in heuristics to pick the right strategy for you. 
    /// The valid values for this parameter are exhaustive, top_values and automatic (default).
    /// Read more: https://typesense.org/docs/latest/api/search.html#faceting-parameters
    /// </summary>
    [JsonPropertyName("facet_strategy")]
    public string? FacetStrategy { get; set; }

    /// <summary>
    /// Maximum number of facet values to be returned.
    /// </summary>
    [JsonPropertyName("max_facet_values")]
    public int? MaxFacetValues { get; set; }

    /// <summary>
    /// Facet values that are returned can now be filtered via this parameter.
    /// The matching facet text is also highlighted. For example, when faceting
    /// by `category`, you can set `facet_query=category:shoe` to return only
    /// facet values that contain the prefix "shoe".
    /// </summary>
    [JsonPropertyName("facet_query")]
    public string? FacetQuery { get; set; }

    /// <summary>
    /// Controls the fuzziness of the facet query filter.
    /// </summary>
    [JsonPropertyName("facet_query_num_typos")]
    public int? FacetQueryNumberTypos { get; set; }

    [JsonPropertyName("facet_return_parent")]
    public string? FacetReturnParent { get; set; }

    /// <summary>
    /// Facet sampling is helpful to improve facet computation speed for large datasets, where the exact count is not required in the UI.
    /// Default: 100 (sampling is disabled by default)
    /// </summary>
    [JsonPropertyName("facet_sample_percent")]
    public int? FacetSamplePercent { get; set; }

    /// <summary>
    /// Facet sampling is helpful to improve facet computation speed for large datasets, where the exact count is not required in the UI.
    /// Default: 0
    /// </summary>
    [JsonPropertyName("facet_sample_threshold")]
    public int? FacetSampleThreshold { get; set; }

    // ---------------------------------------------------------------------------------------
    // Results parameters - https://typesense.org/docs/latest/api/search.html#results-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// List of fields from the document to include in the search result.
    /// </summary>
    [JsonPropertyName("include_fields")]
    public string? IncludeFields { get; set; }

    /// <summary>
    /// List of fields from the document to exclude in the search result.
    /// </summary>
    [JsonPropertyName("exclude_fields")]
    public string? ExcludeFields { get; set; }

    /// <summary>
    /// List of fields which should be highlighted fully without snippeting.
    /// </summary>
    [JsonPropertyName("highlight_full_fields")]
    public string? HighlightFullFields { get; set; }

    /// <summary>
    /// Comma separated list of fields that should be highlighted with snippetting.
    /// Default: all queried fields will be highlighted.
    /// Set to none to disable snippetting fully.
    /// </summary>
    [JsonPropertyName("highlight_fields")]
    public string? HighlightFields { get; set; }

    /// <summary>
    /// The number of tokens that should surround the highlighted text on each side.
    /// </summary>
    [JsonPropertyName("highlight_affix_num_tokens")]
    public int? HighlightAffixNumberOfTokens { get; set; }

    /// <summary>
    /// The start tag used for the highlighted snippets.
    /// </summary>
    [JsonPropertyName("highlight_start_tag")]
    public string? HighlightStartTag { get; set; }

    /// <summary>
    /// The end tag used for the highlighted snippets.
    /// </summary>
    [JsonPropertyName("highlight_end_tag")]
    public string? HighlightEndTag { get; set; }

    /// <summary>
    /// Field values under this length will be fully highlighted, instead of showing
    /// a snippet of relevant portion. Default: 30
    /// </summary>
    [JsonPropertyName("snippet_threshold")]
    public int? SnippetThreshold { get; set; }

    /// <summary>
    /// Maximum number of hits that can be fetched from the collection. Eg: 200
    /// page * per_page should be less than this number for the search request to return results.
    /// A list of custom fields that must be highlighted even if you don't query
    /// for them.
    /// </summary>
    [JsonPropertyName("limit_hits")]
    public int? LimitHits { get; set; }

    /// <summary>
    /// Typesense will attempt to return results early if the cutoff time has elapsed.
    /// This is not a strict guarantee and facet computation is not bound by this parameter.
    /// Default: no search cutoff happens.
    /// </summary>
    [JsonPropertyName("search_cutoff_ms")]
    public int? SearchCutoffMs { get; set; }

    /// <summary>
    /// Control the number of words that Typesense considers for typo and prefix searching.
    /// Default: 4 (or 10000 if exhaustive_search is enabled).
    /// </summary>
    [JsonPropertyName("max_candidates")]
    public int? MaxCandidates { get; set; }

    /// <summary>
    /// Whether all variations of prefixes and typo corrections should be considered,
    /// without stopping early when enough results are found.
    /// Ignores DropTokensThreshold and TypoTokensThreshold.
    /// </summary>
    [JsonPropertyName("exhaustive_search")]
    public bool? ExhaustiveSearch { get; set; }

    // ---------------------------------------------------------------------------------------
    // Typo-Tolerance parameters - https://typesense.org/docs/latest/api/search.html#typo-tolerance-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// The number of typographical errors (1 or 2) that would be tolerated.
    /// </summary>
    [JsonPropertyName("num_typos")]
    public string? NumberOfTypos { get; set; }

    /// <summary>
    /// Minimum word length for 1-typo correction to be applied. The value
    /// of `num_typos` is still treated as the maximum allowed typos.
    /// Default: 4.
    /// </summary>
    [JsonPropertyName("min_len_1typo")]
    public int? MinLen1Typo { get; set; }

    /// <summary>
    /// Minimum word length for 2-typo correction to be applied. The value
    /// of `num_typos` is still treated as the maximum allowed typos.
    /// Default: 7.
    /// </summary>
    [JsonPropertyName("min_len_2typo")]
    public int? MinLen2Typo { get; set; }

    /// <summary>
    /// Treat space as typo: search for q=basket ball if q=basketball is not found or vice-versa.
    /// </summary>
    [JsonPropertyName("split_join_tokens")]
    public SplitJoinTokenOption? SplitJoinTokens { get; set; }

    /// <summary>
    /// If the number of results found for a specific query is less than this number,
    /// Typesense will attempt to look for tokens with more typos until
    /// enough results are found. Default: 100
    /// </summary>
    [JsonPropertyName("typo_tokens_threshold")]
    public int? TypoTokensThreshold { get; set; }

    /// <summary>
    /// If the number of results found for a specific query is less than
    /// this number, Typesense will attempt to drop the tokens in the query until
    /// enough results are found. Tokens that have the least individual hits
    /// are dropped first. Set to 0 to disable. Default: 10
    /// </summary>
    [JsonPropertyName("drop_tokens_threshold")]
    public int? DropTokensThreshold { get; set; }

    /// <summary>
    /// Dictates the direction in which the words in the query must be dropped when the original words in the query do not appear in any document.
    /// 
    /// Values: right_to_left (default), left_to_right, both_sides:3 
    /// A note on both_sides:3 - for queries upto 3 tokens (words) in length, this mode will drop tokens from both sides and exhaustively rank all matching results. 
    /// If query length is greater than 3 words, Typesense will just fallback to default behavior of right_to_left
    /// </summary>
    [JsonPropertyName("drop_tokens_mode")]
    public string? DropTokensMode { get; set; }

    /// <summary>
    /// Set this parameter to false to disable typos on numerical query tokens. Default: true
    /// </summary>
    [JsonPropertyName("enable_typos_for_numerical_tokens")]
    public bool? EnableTyposForNumericalTokens { get; set; }

    /// <summary>
    /// Set this parameter to false to disable typos on alphanumerical query tokens. Default: true
    /// </summary>
    [JsonPropertyName("enable_typos_for_alpha_numerical_tokens")]
    public bool? EnableTyposForAlphaNumericalTokens { get; set; }

    /// <summary>
    /// Allow synonym resolution on typo-corrected words in the query.
    /// Default: 0
    /// </summary>
    [JsonPropertyName("synonym_num_typos")]
    public int? SynonymNumberTypos { get; set; }

    // ---------------------------------------------------------------------------------------
    // Caching parameters - https://typesense.org/docs/latest/api/search.html#caching-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Enable server side caching of search query results. By default, caching is disabled.
    /// Default: false
    /// </summary>
    [JsonPropertyName("use_cache")]
    public bool? UseCache { get; set; }

    /// <summary>
    /// The duration (in seconds) that determines how long the search query is cached.
    /// This value can only be set as part of a scoped API key.
    /// Default: 60
    /// </summary>
    [JsonPropertyName("cache_ttl")]
    public int? CacheTtl { get; set; }

    /// <summary>
    /// How long to wait until an API call to a remote embedding service is considered a timeout.
    /// </summary>
    [JsonPropertyName("remote_embedding_timeout_ms")]
    public int? RemoteEmbeddingTimeoutMs { get; set; }
    
    /// <summary>
    /// When set to true, enables both text match and vector distance scores to be computed for all hybrid search results,
    /// improving the ranking by combining both score types for documents found only by keyword or vector search
    /// Default: false
    /// </summary>
    [JsonPropertyName("rerank_hybrid_matches")]
    public bool? RerankHybridMatches { get; set; }

    /// <summary>
    /// The number of times to retry an API call to a remote embedding service on failure.
    /// </summary>
    [JsonPropertyName("remote_embedding_num_tries")]
    public int? RemoteEmbeddingNumTries { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public SearchParameters()
    {
        Text = string.Empty;
        QueryBy = string.Empty;
    }

    public SearchParameters(string text)
    {
        Text = text;
    }

    public SearchParameters(string text, string queryBy)
    {
        Text = text;
        QueryBy = queryBy;
    }
}

public record GroupedSearchParameters : SearchParameters
{
    // ---------------------------------------------------------------------------------------
    // Grouping parameters - https://typesense.org/docs/latest/api/search.html#grouping-parameters
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// You can aggregate search results into groups or buckets by specify
    /// one or more `group_by` fields. Separate multiple fields with a comma.
    /// To group on a particular field, it must be a faceted field.
    /// </summary>
    [JsonPropertyName("group_by")]
    public string GroupBy { get; set; }

    /// <summary>
    /// Maximum number of hits to be returned for every group. If the `group_limit` is
    /// set as `K` then only the top K hits in each group are returned in the response.
    /// </summary>
    [JsonPropertyName("group_limit")]
    public int? GroupLimit { get; set; }

    /// <summary>
    /// Setting this parameter to true will place all documents that have a null value in the group_by field,
    /// into a single group. Setting this parameter to false, will cause each document with a null value
    /// in the group_by field to not be grouped with other documents.
    /// Default: true
    /// </summary>
    [JsonPropertyName("group_missing_values")]
    public bool? GroupMissingValues { get; set; }

    public GroupedSearchParameters(
        string text,
        string queryBy,
        string groupBy) : base(text, queryBy)
    {
        GroupBy = groupBy;
    }
}
