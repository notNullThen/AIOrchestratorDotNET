namespace AIOrchestrator.Ollama
{
  using System.Text;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using System.Threading.Tasks;

  public class Client
  {
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
      PropertyNameCaseInsensitive = true
    };

    private Stream _stream = Stream.Null;

    public async Task RequestAsync(string prompt, string model)
    {
      string url = "http://localhost:11434/api/generate";

      var client = new HttpClient();
      var json = new { model, prompt };

      var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = new StringContent(
              JsonSerializer.Serialize(json, jsonSerializerOptions),
              Encoding.UTF8,
              "application/json")
      };

      var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
      _stream = await response.Content.ReadAsStreamAsync();
    }

    public ApiResponse GetApiResponse()
    {
      var json = new StreamReader(_stream).ReadLine();
      if (string.IsNullOrWhiteSpace(json)) return new ApiResponse();
      var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, jsonSerializerOptions);
      return apiResponse!;
    }

    public class ApiResponse
    {
      [JsonPropertyName("model")]
      public string Model { get; set; } = string.Empty;

      [JsonPropertyName("created_at")]
      public DateTime CreatedAt { get; set; }

      [JsonPropertyName("response")]
      public string Response { get; set; } = string.Empty;

      [JsonPropertyName("done")]
      public bool Done { get; set; }

      [JsonPropertyName("done_reason")]
      public string DoneReason { get; set; } = string.Empty;

      [JsonPropertyName("context")]
      public List<int> Context { get; set; } = [];

      [JsonPropertyName("total_duration")]
      public long TotalDuration { get; set; }

      [JsonPropertyName("load_duration")]
      public long LoadDuration { get; set; }

      [JsonPropertyName("prompt_eval_count")]
      public int PromptEvalCount { get; set; }

      [JsonPropertyName("prompt_eval_duration")]
      public long PromptEvalDuration { get; set; }

      [JsonPropertyName("eval_count")]
      public int EvalCount { get; set; }

      [JsonPropertyName("eval_duration")]
      public long EvalDuration { get; set; }
    }
  }
}