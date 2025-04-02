namespace AIOrchestrator.Support
{
	using System.Text;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;

	public class OllamaClient
	{
		private const string BaseUrl = "http://localhost:11434";

		private readonly JsonSerializerOptions jsonSerializerOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		private Stream _stream = Stream.Null;

		public async Task RequestAsync(string prompt, Roles role, string model, bool stream = true)
		{
			string url = $"{BaseUrl}/api/generate";

			var client = new HttpClient();
			var requestBody = new { model, prompt, role, stream };
			var requestBodyJson = JsonSerializer.Serialize(requestBody, jsonSerializerOptions);

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(
							requestBodyJson,
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
			var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, jsonSerializerOptions)!;
			return apiResponse;
		}

	}
	public class Message
	{
		[JsonPropertyName("role")]
		public required Roles Role { get; set; }

		[JsonPropertyName("content")]
		public required string Content { get; set; }
	}

	public class ApiRequest
	{
		[JsonPropertyName("model")]
		public string Model { get; set; } = string.Empty;

		[JsonPropertyName("prompt")]
		public string Prompt { get; set; } = string.Empty;

		[JsonPropertyName("stream")]
		public bool Stream { get; set; }
	}

	public enum Roles
	{
		System = 0,
		User = 1,
		Assistant = 2,
		Tool = 3
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