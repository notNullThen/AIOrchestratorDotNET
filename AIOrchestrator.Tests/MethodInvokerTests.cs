namespace AIOrchestratorTests;

using AIOrchestrator.Core;
using AIOrchestrator.Core.Types;
using AIOrchestrator.Utilities;

[TestClass]
public sealed class MethodInvokerTests
{
    [TestMethod]
    public void ExecuteShouldUseEmptyStringForMissingParameter()
    {
        var contextHandler = new ContextHandler<FunctionCallResponse>();
        var errorHandler = new ErrorHandler("test-model", contextHandler);
        var methodInvoker = new MethodInvoker(errorHandler);
        var functionCall = methodInvoker.Deserialize(
            /*lang=json,strict*/
                                 """
            {
              "Function": "JoinValues",
              "Parameters": ["provided"]
            }
            """
        );

        var result = methodInvoker.Execute(functionCall, new TestTarget());

        Assert.AreEqual("provided|", result);
    }

    private sealed class TestTarget
    {
        public static string JoinValues(string first, string second) => $"{first}|{second}";
    }
}
