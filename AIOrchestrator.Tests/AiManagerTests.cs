namespace AIOrchestratorTests;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AIOrchestrator.Core;
using AIOrchestrator.Core.AiAppFacade;
using AIOrchestrator.Core.AiAppFacade.Types;

[TestClass]
public sealed class AiManagerTests
{
    [TestMethod]
    public void ErrorHandlerShouldKeepConversationDiagnostics()
    {
        var manager = new AiManager("test-model", new TestFacade());

        manager.ErrorHandler.SetUserInput("test input");
        manager.ErrorHandler.SetLatestAiOutput("test output");

        var errorMessage = manager.ErrorHandler.GetFullErrorMessage("Failure");

        Assert.AreSame(manager.ErrorHandler, manager.ErrorHandler);
        StringAssert.Contains(errorMessage, "User Input: test input");
        StringAssert.Contains(errorMessage, "Last AI Output: test output");
    }

    [TestMethod]
    public async Task StartAsyncFailureShouldIncludeDiagnosticsOnce()
    {
        const string userInput = "Record my work day";
        const string aiOutput = /*lang=json,strict*/ "{\"function\":\"MissingMethod\",\"parameters\":[]}";
        using var server = new TcpListener(IPAddress.Loopback, 0);
        server.Start();
        var port = ((IPEndPoint)server.LocalEndpoint).Port;
        var responseTask = RespondOnceAsync(server, aiOutput);
        var manager = new AiManager(
            "test-model",
            new TestFacade(),
            ollamaBaseUrl: $"http://127.0.0.1:{port}"
        );

        var exception = await Assert.ThrowsExactlyAsync<Exception>(() =>
            manager.StartAsync(userInput)
        );
        await responseTask;
        var exceptionText = exception.ToString();

        StringAssert.Contains(exceptionText, $"User Input: {userInput}");
        StringAssert.Contains(exceptionText, $"Last AI Output: {aiOutput}");
        Assert.AreEqual(1, CountOccurrences(exceptionText, "User Input:"));
        Assert.AreEqual(1, CountOccurrences(exceptionText, "Last AI Output:"));
    }

    private static async Task RespondOnceAsync(TcpListener server, string aiOutput)
    {
        using var client = await server.AcceptTcpClientAsync();
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

        while (!string.IsNullOrEmpty(await reader.ReadLineAsync()))
        { }

        var responseBody = JsonSerializer.Serialize(
            new
            {
                model = "test-model",
                response = aiOutput,
                done = true,
                done_reason = "stop",
            }
        );
        var responseBytes = Encoding.UTF8.GetBytes(responseBody);
        var headers = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {responseBytes.Length}\r\nConnection: close\r\n\r\n"
        );

        await stream.WriteAsync(headers);
        await stream.WriteAsync(responseBytes);
    }

    private static int CountOccurrences(string value, string text) =>
        value.Split(text, StringSplitOptions.None).Length - 1;

    private sealed class TestFacade : AiAppFacadeBase
    {
        public override AppDescription GetDescription() => [];

        public override string GetConstraints() => string.Empty;
    }
}
