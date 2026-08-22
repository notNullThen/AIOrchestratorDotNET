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

    public ErrorHandler ErrorHandler => new(modelName, _contextHandler);

    private MethodInvoker? _methodInvokerField;
    private MethodInvoker _methodInvoker => _methodInvokerField ??= new MethodInvoker(ErrorHandler);

    private string? _userInput;
    private bool _shouldExit;

    private readonly OllamaClient _ollamaClient = new(ollamaBaseUrl, ollamaHttpTimeout);

    private readonly ContextHandler<FunctionCallResponse> _contextHandler = new();

    private string ManagementPrompt =>
        @$"
SYSTEM:
You are a strict JSON function calling engine. You must output exactly one JSON object and nothing else.

One response is only one function call. Don't output multiple function calls.

Strictly adhere to the following JSON format.
{{
  ""Function"": ""FunctionName"",
  ""Parameters"": [""value"", ""another-value"", ...]
}}

Strictly follow these JSON format types:
- ""Function"": string
- ""Parameters"": string[] (dont put parameter names, put only values in the array)

RULES:
1. Return only a single JSON object.
2. Don't wrap the JSON in Markdown formatting, backticks, or write any text explanations.
3. Call exactly one function per response.
4. Before choosing a function, compare the User request with History and decide whether the requested result is already satisfied.
5. Call `{nameof(appInstance.Exit)}` function if user's request is fulfilled.
6. Operate step-by-step.
7. Evaluate History before deciding the next step.

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

    public async Task ConversationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_shouldExit)
        {
            return;
        }

        try
        {
            var functionsList = await GetFunctionAsync(prompt: ManagementPrompt, cancellationToken);

            foreach (var function in functionsList)
            {
                if (function == null)
                {
                    continue;
                }

                var functionResult = _methodInvoker.Execute(function, appInstance);

                var functionResponse = new FunctionCallResponse
                {
                    Function = function.Function,
                    Parameters = function.Parameters,
                    Response = functionResult,
                };
                _contextHandler.AddToContext(functionResponse);
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

    private async Task<List<FunctionCall?>> GetFunctionAsync(
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

        ErrorHandler.SetLatestAiOutput(response);

        return FunctionsDeserializer.Deserialize<FunctionCall>(response);
    }

    public void Exit()
    {
        _shouldExit = true;
    }
}
