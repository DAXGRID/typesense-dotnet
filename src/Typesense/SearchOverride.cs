using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Typesense.Converter;

namespace Typesense;

public record Exclude
{
	[JsonPropertyName("id")]
	public string Id { get; init; }

	[JsonConstructor]
	public Exclude(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
			throw new ArgumentException("cannot be null or whitespace.", nameof(id));
		Id = id;
	}
}

public record Include
{
	[JsonPropertyName("id")]
	public string Id { get; init; }

	[JsonPropertyName("position")]
	public int Position { get; init; }

	[JsonConstructor]
	public Include(string id, int position)
	{
		if (string.IsNullOrWhiteSpace(id))
			throw new ArgumentException("cannot be null or whitespace.", nameof(id));
		Id = id;
		Position = position;
	}
}

public record Rule
{
	[JsonPropertyName("query")]
	public string? Query { get; init; }

	[JsonPropertyName("match")]
	public string? Match { get; init; }

	[JsonPropertyName("filter_by")]
	public string? FilterBy { get; init; }

	[JsonPropertyName("tags")]
	public IEnumerable<string>? Tags { get; init; }

	[JsonConstructor]
	public Rule(
		string? query = null,
		string? match = null,
		string? filterBy = null,
		IEnumerable<string>? tags = null)
	{
		Match = match;
		Query = query;
		FilterBy = filterBy;
		Tags = tags;
	}
}

public record SearchOverride
{
	[JsonPropertyName("excludes")]
	public IEnumerable<Exclude>? Excludes { get; init; }

	[JsonPropertyName("includes")]
	public IEnumerable<Include>? Includes { get; init; }

	[JsonPropertyName("metadata")]
	public IDictionary<string, object>? Metadata { get; init; }

	[JsonPropertyName("filter_by")]
	public string? FilterBy { get; init; }

	[JsonPropertyName("sort_by")]
	public string? SortBy { get; init; }

	[JsonPropertyName("replace_query")]
	public string? ReplaceQuery { get; init; }

	[JsonPropertyName("remove_matched_tokens")]
	public bool? RemoveMatchedTokens { get; init; }

	[JsonPropertyName("filter_curated_hits")]
	public bool? FilterCuratedHits { get; init; }

	[JsonPropertyName("stop_processing")]
	public bool? StopProcessing { get; init; }

	[JsonPropertyName("effective_from_ts"), JsonConverter(typeof(UnixEpochDateTimeConverter))]
	public DateTime? EffectiveFromTs { get; init; }

	[JsonPropertyName("effective_to_ts"), JsonConverter(typeof(UnixEpochDateTimeConverter))]
	public DateTime? EffectiveToTs { get; init; }

	[JsonPropertyName("rule")]
	public Rule Rule { get; init; }

	public SearchOverride(Rule rule)
	{
		ArgumentNullException.ThrowIfNull(rule);
		Rule = rule;
	}
}
