using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public record SearchOverrideResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("excludes")]
    public IEnumerable<Exclude> Excludes { get; init; }

    [JsonPropertyName("includes")]
    public IEnumerable<Include> Includes { get; init; }

    [JsonPropertyName("metadata")]
    public IDictionary<string, object> Metadata { get; init; }

    [JsonPropertyName("filter_by")]
    public string FilterBy { get; init; }

    [JsonPropertyName("sort_by")]
    public string SortBy { get; init; }

    [JsonPropertyName("replace_query")]
    public string ReplaceQuery { get; init; }

    [JsonPropertyName("remove_matched_tokens")]
    public bool RemoveMatchedTokens { get; init; }

    [JsonPropertyName("filter_curated_hits")]
    public bool FilterCuratedHits { get; init; }

    [JsonPropertyName("stop_processing")]
    public bool StopProcessing { get; init; }

    [JsonPropertyName("effective_from_ts"), JsonConverter(typeof(UnixEpochDateTimeLongConverter))]
    public DateTime EffectiveFromTs { get; init; }

    [JsonPropertyName("effective_to_ts"), JsonConverter(typeof(UnixEpochDateTimeLongConverter))]
    public DateTime EffectiveToTs { get; init; }

    [JsonPropertyName("rule")]
    public Rule Rule { get; init; }

    [JsonConstructor]
    public SearchOverrideResponse(IEnumerable<Exclude> excludes, IEnumerable<Include> includes,
        IDictionary<string, object> metadata, string filterBy, string sortBy, string replaceQuery, bool removeMatchedTokens,
        bool filterCuratedHits, bool stopProcessing, DateTime effectiveFromTs, DateTime effectiveToTs,
        Rule rule, string id)
    {
        Excludes = excludes;
        Includes = includes;
        Metadata = metadata;
        FilterBy = filterBy;
        SortBy = sortBy;
        ReplaceQuery = replaceQuery;
        RemoveMatchedTokens = removeMatchedTokens;
        FilterCuratedHits = filterCuratedHits;
        StopProcessing = stopProcessing;
        EffectiveFromTs = effectiveFromTs;
        EffectiveToTs = effectiveToTs;
        Rule = rule;
        Id = id;
    }
}
