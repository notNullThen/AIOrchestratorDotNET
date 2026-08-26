namespace AIOrchestrator.Core.Types;

using System.Text.Json.Serialization;

public class FunctionCall
{
    [JsonPropertyOrder(1)]
    public required string Function { get; init; }

    [JsonPropertyOrder(2)]
    public string[] Parameters { get; init; } = [];
}
