using AiOrchestrator;

class Program
{
	static async Task Main(string[] args)
	{
		await OllamaAi.AiRequestAsync("Write any 5 words. Nothing else. In 1 line.");
	}
}