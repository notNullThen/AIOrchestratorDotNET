namespace AIOrchestratorTests;

using AIOrchestrator.Core;
using AIOrchestrator.Core.Types;

[TestClass]
public sealed class MethodInvokerTests
{
    [TestMethod]
    public void ExecuteShouldUseEmptyStringForMissingParameter()
    {
        var methodInvoker = new MethodInvoker();
        var functionCall = new FunctionCall
        {
            Function = nameof(TestTarget.JoinValues),
            Parameters = ["provided"],
        };

        var result = methodInvoker.Execute(functionCall, new TestTarget());

        Assert.AreEqual("provided|", result);
    }

    private sealed class TestTarget
    {
        public static string JoinValues(string first, string second) => $"{first}|{second}";
    }
}
