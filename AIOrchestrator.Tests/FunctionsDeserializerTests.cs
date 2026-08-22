namespace AIOrchestratorTests;

using AIOrchestrator.Core;

[TestClass]
public sealed class FunctionsDeserializerTests
{
    [TestMethod]
    public void DeserializeShouldSplitCallsWithParametersOrFunctionOnlyEndings()
    {
        const string response =
            /*lang=json,strict*/
                                 "{\"function\":\"WithParameters\",\"parameters\":[\"value\"]} "
            + /*lang=json,strict*/ "{\"function\":\"WithoutParameters\"}";

        var functions = FunctionsDeserializer.Deserialize(response);

        Assert.HasCount(2, functions);
        Assert.IsNotNull(functions[0]);
        Assert.AreEqual("WithParameters", functions[0]!.Function);
        Assert.HasCount(1, functions[0]!.Parameters);
        Assert.AreEqual("value", functions[0]!.Parameters[0]);
        Assert.IsNotNull(functions[1]);
        Assert.AreEqual("WithoutParameters", functions[1]!.Function);
        Assert.HasCount(0, functions[1]!.Parameters);
    }

    [TestMethod]
    public void DeserializeShouldAllowWhitespaceBeforeEitherClosingBrace()
    {
        const string response =
            /*lang=json,strict*/
                                 "{ \"function\" : \"WithParameters\", \"parameters\" : []   } "
            + /*lang=json,strict*/ "{ \"function\" : \"WithoutParameters\"   }";

        var functions = FunctionsDeserializer.Deserialize(response);

        Assert.HasCount(2, functions);
        Assert.AreEqual("WithParameters", functions[0]?.Function);
        Assert.AreEqual("WithoutParameters", functions[1]?.Function);
    }
}
