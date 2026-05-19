namespace AIOrchestrator.Core;

using AIOrchestrator.Core.Types;

public class ErrorHandler(string modelName, ContextHandler<FunctionCallResponse> contextHandler)
{
    private string _userInput = null!;
    private string _latestAiOutput = null!;

    public void SetUserInput(string userInput) =>
        _userInput = string.IsNullOrEmpty(userInput) ? _userInput : userInput;

    public void SetLatestAiOutput(string latestAiOutput) =>
        _latestAiOutput = string.IsNullOrEmpty(latestAiOutput) ? _latestAiOutput : latestAiOutput;

    public string GetFullErrorMessage(string message) => $"{message}\n\n{GetContext()}";

    private string GetContext() =>
        @$"
Using the ""{modelName}"" LLM.
User Input: {_userInput ?? "Not set"}
Last AI Output: {_latestAiOutput ?? "Not set"}
Full Context History:
{contextHandler.GetContextJson()}
";
}
