using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public sealed record StatsResponse
{
    [JsonPropertyName("delete_latency_ms")]
    public decimal DeleteLatencyMs { get; init; }

    [JsonPropertyName("delete_requests_per_second")]
    public decimal DeleteRequestsPerSecond { get; init; }

    [JsonPropertyName("import_latency_ms")]
    public decimal ImportLatencyMs { get; init; }

    [JsonPropertyName("import_requests_per_second")]
    public decimal ImportRequestsPerSecond { get; init; }

    [JsonPropertyName("latency_ms")]
    public Dictionary<string, decimal> LatencyMs { get; init; }

    [JsonPropertyName("pending_write_batches")]
    public decimal PendingWriteBatches { get; init; }

    [JsonPropertyName("requests_per_second")]
    public Dictionary<string, decimal> RequestsPerSecond { get; init; }

    [JsonPropertyName("search_latency_ms")]
    public decimal SearchLatencyMs { get; init; }

    [JsonPropertyName("search_requests_per_second")]
    public decimal SearchRequestsPerSecond { get; init; }

    [JsonPropertyName("total_requests_per_second")]
    public decimal TotalRequestsPerSecond { get; init; }

    [JsonPropertyName("write_latency_ms")]
    public decimal WriteLatencyMs { get; init; }

    [JsonPropertyName("write_requests_per_second")]
    public decimal WriteRequestsPerSecond { get; init; }

    [JsonConstructor]
    public StatsResponse(
        decimal deleteLatencyMs,
        decimal deleteRequestsPerSecond,
        decimal importLatencyMs,
        decimal importRequestsPerSecond,
        Dictionary<string, decimal> latencyMs,
        decimal pendingWriteBatches,
        Dictionary<string, decimal> requestsPerSecond,
        decimal searchLatencyMs,
        decimal searchRequestsPerSecond,
        decimal totalRequestsPerSecond,
        decimal writeLatencyMs,
        decimal writeRequestsPerSecond
    )
    {
        DeleteLatencyMs = deleteLatencyMs;
        DeleteRequestsPerSecond = deleteRequestsPerSecond;
        ImportLatencyMs = importLatencyMs;
        ImportRequestsPerSecond = importRequestsPerSecond;
        LatencyMs = latencyMs;
        PendingWriteBatches = pendingWriteBatches;
        RequestsPerSecond = requestsPerSecond;
        SearchLatencyMs = searchLatencyMs;
        SearchRequestsPerSecond = searchRequestsPerSecond;
        TotalRequestsPerSecond = totalRequestsPerSecond;
        WriteLatencyMs = writeLatencyMs;
        WriteRequestsPerSecond = writeRequestsPerSecond;
    }
}
