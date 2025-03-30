
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIOrchestrator.Ollama
{
  public static class Client
  {
    public static async Task AiRequestAsync(string prompt, string model = "mistral")
    {
      string url = "http://localhost:11434/api/generate";

      using var client = new HttpClient();
      var json = new { model, prompt };

      var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = new StringContent(
              JsonSerializer.Serialize(json),
              Encoding.UTF8,
              "application/json")
      };

      using var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
      using var stream = await response.Content.ReadAsStreamAsync();
      using var reader = new StreamReader(stream);

      while (!reader.EndOfStream)
      {
        string? line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line)) continue;

        var lineResponse = JsonSerializer.Deserialize<ApiResponse>(line);
        if (lineResponse?.Response != null)
        {
          Console.Write(lineResponse.Response);
        }
      }
    }

    private class ApiResponse
    {
      [JsonPropertyName("model")]
      public required string Model { get; set; }

      [JsonPropertyName("created_at")]
      public required DateTime CreatedAt { get; set; }

      [JsonPropertyName("response")]
      public required string Response { get; set; }

      [JsonPropertyName("done")]
      public required bool Done { get; set; }

    }
  }
}