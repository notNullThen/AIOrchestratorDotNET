namespace AIOrchestrator.ContextHandler
{
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using Ollama;

  public class ChatHandler
  {
    private readonly OllamaClient _ollamaClient = new();
    private readonly List<Message> messages = [];
    private List<string> _promptParts = [];

    private static readonly JsonSerializerOptions promptJsonSerializerOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
      Converters =
      {
        new JsonStringEnumConverter()
      }
    };


    public async Task ConversationHandlerAsync(string model = "mistral")
    {
      Console.Write("You:\n  ");
      string userInput = Console.ReadLine()!;
      if (userInput == "exit")
      {
        return;
      }
      messages.Add(new() { Role = Roles.User, Content = userInput });
      string messagesJson = JsonSerializer.Serialize(messages, promptJsonSerializerOptions);

      /* PROMPT */

      // Beginning
      _promptParts.Add($"{Roles.System} message:");

      // Conversation history
      _promptParts.Add(@$"
You are {Roles.Assistant}.
Conversation history is a JSON array of messages with defined roles.
Your conversation history:
{messagesJson}");

      // Task
      _promptParts.Add(@$"
Your task is to respond to the user's input in a conversational manner.
Your responses should be **short and laconic**. Don't use quotes in start and end of your response.");

      string prompt = string.Join("\n", _promptParts);

      await _ollamaClient.RequestAsync(prompt, Roles.System, model, stream: true);

      Console.Write("ChatBot:\n  ");
      string content = string.Empty;
      while (true)
      {
        var line = _ollamaClient.GetApiResponse();
        var response = line.Response;

        content += response;
        Console.Write(response);
        if (line.Done)
        {
          Console.Write("\n\n");
          break;
        }
      }

      messages.Add(new() { Role = Roles.Assistant, Content = content });

      await ConversationHandlerAsync(model);
    }
  }
}