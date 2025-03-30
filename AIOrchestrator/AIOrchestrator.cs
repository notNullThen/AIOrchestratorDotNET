namespace AIOrchestrator;

using ContextHandler;

public class Chat
{
  private readonly ChatHandler _chatHandler = new();
  public async Task StartChatAsync()
  {
    await _chatHandler.ConversationHandlerAsync();
  }
}
