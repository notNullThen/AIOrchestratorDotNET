namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

public class ApiRequestOptions
{
    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; init; }
}
