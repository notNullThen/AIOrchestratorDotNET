namespace AIOrchestratorTests;

using System.Text.Encodings.Web;
using System.Text.Json;
using AIOrchestrator.Core;
using AIOrchestrator.Core.AiAppFacade;

public abstract class AiTestsBase
{
    public required TestContext TestContext { get; set; }

    public required AiManager AiManager { get; set; }
    protected abstract string ModelName { get; }
    protected abstract AiAppFacadeBase AppInstance { get; }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [TestInitialize]
    public void Setup()
    {
        AiManager = new AiManager(modelName: ModelName, appInstance: AppInstance);

        SetLoopDetection(contextCountLimit: 10);
    }

    [TestCleanup]
    public void LogContextOnFailure()
    {
        if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
        {
            if (AiManager?.ContextHandler?.Context != null)
            {
                var json = JsonSerializer.Serialize(AiManager.ContextHandler.Context, _jsonOptions);

                TestContext.WriteLine("--- TEST FAILED: FUNCTION CALLS AI CONTEXT ---");
                TestContext.WriteLine(json);
                TestContext.WriteLine("---------------------------------------");
            }
        }
    }

    protected void SetLoopDetection(int contextCountLimit)
    {
        if (AiManager == null)
        {
            throw new InvalidOperationException(
                AiManager!.ErrorHandler.GetFullErrorMessage(
                    $"{nameof(AiManager)} must be set before setting loop detection."
                )
            );
        }

        AiManager.ContextHandler.OnContextUpdated += (sender, context) =>
        {
            if (context.Count > contextCountLimit)
            {
                throw new InvalidOperationException(
                    AiManager!.ErrorHandler.GetFullErrorMessage(
                        $"Seems like we went into a loop! Context count is more than {contextCountLimit}. "
                    )
                );
            }
        };
    }
}
