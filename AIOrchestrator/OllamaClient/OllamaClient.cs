namespace AIOrchestrator.OllamaClient;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Types;

internal sealed class OllamaClient
{
    public OllamaClient(string? baseUrl, TimeSpan? timeout = null)
    {
        _timeout = timeout;
        _baseUrl = baseUrl ?? "http://localhost:11434";
    }

    private readonly TimeSpan? _timeout;

    private readonly string _baseUrl;

    private HttpClient _httpClient =>
        _timeout is null ? new() : new() { Timeout = (TimeSpan)_timeout };

    public async Task<OllamaTagsResponse> GetTagsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{_baseUrl}/api/tags";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Couldn't fetch tags from Ollama server", ex);
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            return JsonSerializer.Deserialize<OllamaTagsResponse>(responseJson)
                ?? new OllamaTagsResponse();
        }
        catch (JsonException ex)
        {
            throw new Exception(
                $"Failed to deserialize Ollama tags response. Content: {responseJson}",
                ex
            );
        }
    }

    public async Task<ApiResponse> RequestAsync(
        string prompt,
        string model,
        Role role = Role.User,
        ApiRequestOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var requestMessage = GetRequestMessage(
            url: $"{_baseUrl}/api/generate",
            request: new()
            {
                Model = model,
                Prompt = prompt,
                Role = role.ToString(),
                Stream = false,
                Options = options,
            }
        );

        return await GetResponseAsync(requestMessage, cancellationToken);
    }

    public static HttpRequestMessage GetRequestMessage(string url, ApiRequest request)
    {
        var requestBodyJson = JsonSerializer.Serialize(request);
        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json"),
        };
    }

    public async Task<ApiResponse> GetResponseAsync(
        HttpRequestMessage requestMessage,
        CancellationToken cancellationToken = default
    )
    {
        HttpResponseMessage response;
        var responseJson = string.Empty;
        try
        {
            response = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == null)
            {
                throw new Exception("Couldn't connect to Ollama server", ex);
            }

            throw new Exception($"Ollama API error: {ex.Message} Response: {responseJson}", ex);
        }

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new Exception("Ollama API returned an empty response.");
        }

        try
        {
            return JsonSerializer.Deserialize<ApiResponse>(responseJson)!;
        }
        catch (JsonException ex)
        {
            throw new Exception(
                $"Failed to deserialize Ollama API response. Content: {responseJson}",
                ex
            );
        }
    }
}
