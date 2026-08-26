namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

public sealed class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel> Models { get; init; } = [];
}

public sealed class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; init; }

    [JsonPropertyName("details")]
    public OllamaModelDetails? Details { get; init; }
}

public sealed class OllamaModelDetails
{
    [JsonPropertyName("parameter_size")]
    public string ParameterSize { get; init; } = string.Empty;

    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; init; } = string.Empty;
}
