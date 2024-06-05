using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Typesense;

public record AutoEmbeddingConfig
{
    [JsonPropertyName("from")]
    public Collection<string> From { get; init; }

    [JsonPropertyName("model_config")]
    public ModelConfig ModelConfig { get; init; }

    public AutoEmbeddingConfig(Collection<string> from, ModelConfig modelConfig)
    {
        From = from;
        ModelConfig = modelConfig;
    }
}

public record ModelConfig
{
    [JsonPropertyName("model_name")]
    public string ModelName { get; init; }

    [JsonPropertyName("api_key")]
    public string? ApiKey { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; init; }

    [JsonPropertyName("project_id")]
    public string? ProjectId { get; init; }

    [JsonPropertyName("indexing_prefix")]
    public string? IndexingPrefix { get; init; }

    [JsonPropertyName("query_prefix")]
    public string? QueryPrefix { get; init; }

    public ModelConfig(string modelName)
    {
        ModelName = modelName;
    }
}
