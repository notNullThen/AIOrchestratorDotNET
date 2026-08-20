namespace AIOrchestrator.Core;

using AiAppFacade;
using OllamaClient;
using OllamaClient.Types;
using Types;
using Utilities;

public sealed class AiManager(
    string modelName,
    AiAppFacadeBase appInstance,
    ApiRequestOptions? options = null,
    string? ollamaBaseUrl = null,
    TimeSpan? ollamaHttpTimeout = null
)
{
    public ContextHandler<FunctionCallResponse> ContextHandler => _contextHandler;

    private ErrorHandler? _errorHandler;
    public ErrorHandler ErrorHandler =>
        _errorHandler ??= new ErrorHandler(modelName, _contextHandler);

    private MethodInvoker? _methodInvokerField;
    private MethodInvoker _methodInvoker => _methodInvokerField ??= new MethodInvoker(ErrorHandler);

    private bool Debug { get; set; }

    private string? _userInput;
    private object? _aiOutput;
    private bool _shouldExit;

    private readonly OllamaClient _ollamaClient = new(ollamaBaseUrl, ollamaHttpTimeout);

    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();

    private string ManagementPrompt =>
        @$"
SYSTEM:
You are a strict JSON function calling engine. You must output EXACTLY ONE JSON object and NOTHING else.

YOU MUST strictly adhere to the following JSON format.
{{
  ""Function"": ""FunctionName"",
  ""Parameters"": [""value"", ""another-value"", ...]
}}

You MUST strictly follow these JSON format types:
- ""Function"": string
- ""Parameters"": string[] (only values, no parameter names)

RULES:
1. You MUST return ONLY a single JSON object.
2. You MUST NOT wrap the JSON in Markdown formatting, backticks, or write any text explanations.
3. You MUST call EXACTLY ONE function per response.
4. You MUST use ONLY functions from the FUNCTIONS list.
5. Before choosing a function, you MUST compare the User request with History and decide whether the requested result is already satisfied.
6. If History already contains the requested result, or the task is otherwise fully completed, you MUST immediately call {nameof(appInstance.Exit)} with no parameters. This is the ONLY valid action in that situation. Do NOT repeat a previous function or call another function.
7. The required completion response is exactly:
{{
  ""Function"": ""{nameof(appInstance.Exit)}"",
  ""Parameters"": []
}}
8. You MUST NOT call {nameof(appInstance.Exit)} while another function call is still required to satisfy the User request.
9. You MUST operate step-by-step.
10. You MUST evaluate History before deciding the next step.

FUNCTIONS:
{appInstance.GetDescription()}

CONSTRAINTS:
{appInstance.GetConstraints()}

STATE:
User: {_userInput}
History: {_contextHandler.GetContextJson()}

You MUST process the STATE and reply with EXACTLY ONE JSON function call. If the STATE already satisfies the User request, that call MUST be {nameof(appInstance.Exit)}.
";

    public string GetManagementPrompt() => ManagementPrompt;

    public void SetDebug(bool debug) => Debug = debug;

    public async Task ConversationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_shouldExit)
        {
            return;
        }

        try
        {
            var function = await GetFunctionAsync(prompt: ManagementPrompt, cancellationToken);

            _aiOutput = _methodInvoker.Execute(function, appInstance);

            var functionResponse = new FunctionCallResponse
            {
                Function = function.Function,
                Parameters = function.Parameters,
                Response = _aiOutput,
            };
            _contextHandler.AddToContext(functionResponse);
            if (Debug)
            {
                Console.WriteLine(_contextHandler.GetLastContextPartJson());
            }

            await ConversationAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _shouldExit = true;
            throw;
        }
        catch (Exception ex)
        {
            _shouldExit = true;
            throw new Exception(
                ErrorHandler.GetFullErrorMessage(
                    "An error occurred during AI conversation execution."
                ),
                ex
            );
        }
    }

    public async Task StartAsync(string userInput, CancellationToken cancellationToken = default)
    {
        _userInput = userInput;
        _shouldExit = false;
        ErrorHandler.SetUserInput(userInput);

        appInstance.OnExit = Exit;
        await ConversationAsync(cancellationToken);
    }

    private async Task<FunctionCall> GetFunctionAsync(
        string prompt,
        CancellationToken cancellationToken = default
    )
    {
        var apiOptions =
            options == null
                ? null
                : new ApiRequestOptions
                {
                    Temperature = options.Temperature,
                    NumPredict = options.NumPredict,
                };

        var ollamaResponse = await _ollamaClient.RequestAsync(
            prompt: prompt,
            model: modelName,
            options: apiOptions,
            cancellationToken: cancellationToken
        );

        var response = ollamaResponse.Response;
        var functionJson = MarkdownProcess.RemoveCodeMarkdown(response);

        ErrorHandler.SetLatestAiOutput(response);

        return _methodInvoker.Deserialize(functionJson);
    }

    public void Exit()
    {
        Console.WriteLine($"\nOutput:\n{_aiOutput}");
        _shouldExit = true;
    }
}
