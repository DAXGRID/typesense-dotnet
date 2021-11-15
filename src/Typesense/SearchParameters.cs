using System;
using System.Text.Json.Serialization;

namespace Typesense;
public class SearchParameters
{
    [JsonPropertyName("q")]
    public string Text { get; set; }
    [JsonPropertyName("query_by")]
    public string QueryBy { get; set; }
    [JsonPropertyName("query_by_weights")]
    public string QueryByWeights { get; set; }
    [Obsolete("max_hits has been deprecated since Typesense version 0.19.0")]
    [JsonPropertyName("max_hits")]
    public string MaxHits { get; set; }
    [JsonPropertyName("prefix")]
    public string Prefix { get; set; }
    [JsonPropertyName("filter_by")]
    public string FilterBy { get; set; }
    [JsonPropertyName("sort_by")]
    public string SortBy { get; set; }
    [JsonPropertyName("facet_by")]
    public string FacetBy { get; set; }
    [JsonPropertyName("max_facet_values")]
    public string MaxFacetValues { get; set; }
    [JsonPropertyName("facet_query")]
    public string FacetQuery { get; set; }
    [JsonPropertyName("num_typos")]
    public string NumberOfTypos { get; set; }
    [JsonPropertyName("page")]
    public string Page { get; set; }
    [JsonPropertyName("per_page")]
    public string PerPage { get; set; }
    [JsonPropertyName("group_by")]
    public string GroupBy { get; set; }
    [JsonPropertyName("group_limit")]
    public string GroupLimit { get; set; }
    [JsonPropertyName("include_fields")]
    public string IncludeFields { get; set; }
    [JsonPropertyName("exclude_fields")]
    public string ExcludeFields { get; set; }
    [JsonPropertyName("highlight_full_fields")]
    public string HighlightFullFields { get; set; }
    [JsonPropertyName("highlight_affix_num_tokens")]
    public string HighlightAffixNumberOfTokens { get; set; }
    [JsonPropertyName("highlight_start_tag")]
    public string HighlightStartTag { get; set; }
    [JsonPropertyName("highlight_end_tag")]
    public string HighlightEndTag { get; set; }
    [JsonPropertyName("snippet_threshold")]
    public string SnippetThreshold { get; set; }
    [JsonPropertyName("drop_tokens_threshold")]
    public string DropTokensThreshold { get; set; }
    [JsonPropertyName("typo_tokens_threshold")]
    public string TypoTokensThreshold { get; set; }
    [JsonPropertyName("pinned_hits")]
    public string PinnedHits { get; set; }
    [JsonPropertyName("hidden_hits")]
    public string HiddenHits { get; set; }
    [JsonPropertyName("limit_hits")]
    public string LimitHits { get; set; }
    [JsonPropertyName("pre_segmented_query")]
    public bool? PreSegmentedQuery { get; set; }
    [JsonPropertyName("enable_overrides")]
    public bool? EnableOverrides { get; set; }
}
