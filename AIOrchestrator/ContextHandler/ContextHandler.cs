namespace AIOrchestrator.ContextHandler
{
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using Ollama;

  public class ChatHandler
  {
    private readonly OllamaClient _ollamaClient = new();
    private readonly List<Message> messages = [];

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

      string messagesJson = JsonSerializer.Serialize(messages, promptJsonSerializerOptions);
      string prompt = @$"
{Roles.System} message:
Conversation history is a JSON array of messages, where ""Role"": 0 is User's message and ""Role"": 1 is your message.

Your conversation history:
{messagesJson}

Your task is to respond to the user's input in a conversational manner.

User input: {userInput}

Your responses should be **short and laconic**. Your response should be just a text - a continuation of dialogue.
";

      messages.Add(new() { Role = Roles.User, Content = userInput });

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