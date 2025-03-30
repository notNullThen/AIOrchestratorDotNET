
using AIOrchestrator.Ollama;

namespace AIOrchestrator
{
  public static class Chat
  {
    public static async Task StartChatAsync(string prompt)
    {
      await Client.AiRequestAsync(prompt);
    }
  }
}
