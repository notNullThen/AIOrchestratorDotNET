namespace AIOrchestrator.Core;

using System.Text.Json;
using System.Text.RegularExpressions;

public static class FunctionsDeserializer
{
    private const string FunctionStart = "{\"function\":\"";
    private const string FunctionEnd = "\"}";

    private static readonly Regex _functionStartRegex = FunctionsDeserializerRegex.FunctionStart();
    private static readonly Regex _functionEndRegex = FunctionsDeserializerRegex.FunctionEnd();

    private static readonly JsonSerializerOptions _caseInsensitiveJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static List<T?> Deserialize<T>(string aiResponse)
    {
        var functionsJsonList = GetFunctionsJsonList(aiResponse);

        var functions = functionsJsonList
            .Select(json => JsonSerializer.Deserialize<T>(json, _caseInsensitiveJson))
            .ToList();

        return functions;
    }

    private static List<string> GetFunctionsJsonList(string aiResponse)
    {
        var rawTextList = _functionStartRegex.Split(aiResponse).Skip(1).ToList();

        var functionsJsonList = rawTextList
            .Select(rawText =>
            {
                var rawTextWithStart = string.Concat(FunctionStart, rawText);
                return string.Concat(_functionEndRegex.Split(rawTextWithStart)[0], FunctionEnd);
            })
            .ToList();

        return functionsJsonList;
    }
}
