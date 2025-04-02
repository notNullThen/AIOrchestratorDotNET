using AIOrchestrator;

class Program
{
	static async Task Main(string[] args)
	{
		await new AIOrchestrator.AIOrchestrator().StartChatAsync();
	}
}
