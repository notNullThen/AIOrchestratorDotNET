namespace AIOrchestrator.OllamaClient.Types;

using System.Text.Json.Serialization;

internal sealed class ApiResponse
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("response")]
    public required string Response { get; init; }

    [JsonPropertyName("done")]
    public bool Done { get; init; }

    [JsonPropertyName("done_reason")]
    public required string DoneReason { get; init; }

    [JsonPropertyName("context")]
    public List<int> Context { get; init; } = [];

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; init; }

    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; init; }

    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; init; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; init; }

    [JsonPropertyName("eval_count")]
    public int EvalCount { get; init; }

    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; init; }
}
