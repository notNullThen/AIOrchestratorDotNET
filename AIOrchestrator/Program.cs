using AIOrchestrator;

class Program
{
	static async Task Main(string[] args)
	{
		// await Chat.StartChatAsync("Write any 5 words. Nothing else. In 1 line.");
		await new Chat().StartChatAsync();
	}
}
