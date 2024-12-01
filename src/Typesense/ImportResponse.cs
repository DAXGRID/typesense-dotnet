using System.Text.Json.Serialization;

namespace Typesense;

public record ImportResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("document")]
    public string? Document { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonConstructor]
    public ImportResponse(bool success, string? error = null, string? document = null, string? id = null)
    {
        Success = success;
        Error = error;
        Document = document;
        Id = id;
    }
}
