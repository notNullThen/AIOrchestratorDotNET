namespace AIOrchestrator.Core;

using System.Text.Json;
using System.Text.RegularExpressions;
using AIOrchestrator.Core.Types;

public static class FunctionsDeserializer
{
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
        var functionsJsonList = new List<string>();

        foreach (Match functionStart in _functionCallStartRegex.Matches(aiResponse))
        {
            var functionEnd = _functionCallEndRegex.Match(
                aiResponse,
                functionStart.Index + functionStart.Length
            );

            if (!functionEnd.Success)
            {
                throw new JsonException("A function call has no valid ending.");
            }

            functionsJsonList.Add(
                aiResponse[functionStart.Index..(functionEnd.Index + functionEnd.Length)]
            );
        }

        return functionsJsonList;
    }
}
