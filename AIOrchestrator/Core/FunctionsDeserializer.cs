namespace AIOrchestrator.Core;

using System.Text.Json;
using System.Text.RegularExpressions;
using AIOrchestrator.Core.Types;

public static class FunctionsDeserializer
{
    private const string FunctionStart = "{\"function\":\"";
    private const string FunctionEnd = "]}";

    private static readonly Regex _functionCallStartRegex =
        FunctionsDeserializerRegex.FunctionCallStart();
    private static readonly Regex _functionCallEndRegex =
        FunctionsDeserializerRegex.FunctionCallEnd();

    private static readonly JsonSerializerOptions _caseInsensitiveJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static List<FunctionCall?> Deserialize(string aiResponse)
    {
        try
        {
            var functionsJsonList = GetFunctionsJsonList(aiResponse);

            return
            [
                .. functionsJsonList.Select(json =>
                    JsonSerializer.Deserialize<FunctionCall>(json, _caseInsensitiveJson)
                ),
            ];
        }
        catch (Exception exception)
        {
            throw new JsonException(
                $"Failed to deserialize AI response into {nameof(FunctionCall)}. "
                    + $"AI response: {aiResponse}",
                exception
            );
        }
    }

    private static List<string> GetFunctionsJsonList(string aiResponse)
    {
        var rawTextList = _functionCallStartRegex.Split(aiResponse).Skip(1).ToList();

        var functionsJsonList = rawTextList
            .Select(rawText =>
            {
                var rawTextWithStart = string.Concat(FunctionStart, rawText);
                return string.Concat(_functionCallEndRegex.Split(rawTextWithStart)[0], FunctionEnd);
            })
            .ToList();

        return functionsJsonList;
    }
}
