namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

internal sealed class ApiRequest
{
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }

    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("options")]
    public ApiRequestOptions? Options { get; init; }
}
