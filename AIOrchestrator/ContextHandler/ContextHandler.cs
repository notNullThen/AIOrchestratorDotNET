using System.Text.Json;

namespace AIOrchestrator.ContextHandler
{
  public class ChatHandler
  {
    private readonly Ollama.Client _ollamaClient = new();
    private readonly List<Message> messages = [];
    private static readonly JsonSerializerOptions promptJsonSerializerOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true
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
Conversation history is a JSON array of messages, where ""Role"": 0 is User's message and ""Role"": 1 is your message.

Your conversation history:
{messagesJson}

Your task is to respond to the user's input in a conversational manner.

User input: {userInput}

Your responses should be **very short and laconic**. Your response should be just text.
";

      messages.Add(new() { Role = MessageRole.User, Content = userInput });

      await _ollamaClient.RequestAsync(prompt, model);

      Console.Write("ChatBot:\n ");
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

      messages.Add(new() { Role = MessageRole.Chatbot, Content = content });

      await ConversationHandlerAsync(model);
    }

    private class Message
    {
      public required MessageRole Role { get; set; }
      public required string Content { get; set; }
    }

    private enum MessageRole
    {
      User,
      Chatbot
    }
  }
}