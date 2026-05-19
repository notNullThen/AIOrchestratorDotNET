namespace AIOrchestrator.OllamaClient;

using Types;

/// <summary>
/// Lightweight helper for querying available Ollama models without
/// needing a full <see cref="Core.AiManager"/> instance.
/// </summary>
public static class OllamaModels
{
    public static async Task<List<OllamaModel>> GetModelsAsync(
        string baseUrl = "http://localhost:11434",
        TimeSpan? timeout = null
    )
    {
        var client = new OllamaClient(baseUrl, timeout);

        var tags = await client.GetTagsAsync();
        return tags.Models;
    }
}
