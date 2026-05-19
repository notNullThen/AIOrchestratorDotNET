namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

public sealed class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel> Models { get; set; } = [];
}

public sealed class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("details")]
    public OllamaModelDetails? Details { get; set; }
}

public sealed class OllamaModelDetails
{
    [JsonPropertyName("parameter_size")]
    public string ParameterSize { get; set; } = string.Empty;

    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; set; } = string.Empty;
}
