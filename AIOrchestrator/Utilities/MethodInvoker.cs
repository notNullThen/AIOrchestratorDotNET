namespace AIOrchestrator.Utilities;

using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using AIOrchestrator.Core;
using Core.Types;

internal class MethodInvoker(ErrorHandler errorHandler)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly JsonSerializerOptions _prettyJsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public object Execute<T>(FunctionCall instruction, T targetInstance)
    {
        errorHandler.SetLatestAiOutput(
            JsonSerializer.Serialize(instruction, _prettyJsonSerializerOptions)
        );

        try
        {
            var method =
                targetInstance!
                    .GetType()
                    .GetMethod(
                        instruction.Function,
                        BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                    )
                ?? throw new MissingMethodException(
                    errorHandler.GetFullErrorMessage(
                        $"Method {instruction.Function}() not found in {targetInstance.GetType().Name} class."
                    )
                );

            var parameters = ConvertParametersForMethod(instruction.Parameters, method);

            return method.Invoke(targetInstance, parameters)!;
        }
        catch (Exception ex)
        {
            throw new Exception(
                errorHandler.GetFullErrorMessage(
                    $"Error executing method instructions:\n{JsonSerializer.Serialize(instruction, _prettyJsonSerializerOptions)}\n"
                ),
                ex
            );
        }
    }

    public FunctionCall Deserialize(string jsonInstruction)
    {
        errorHandler.SetLatestAiOutput(jsonInstruction);

        if (string.IsNullOrWhiteSpace(jsonInstruction))
        {
            throw new ArgumentException(
                errorHandler.GetFullErrorMessage(
                    "AI response resulted in an empty JSON instruction."
                ),
                nameof(jsonInstruction)
            );
        }

        try
        {
            return JsonSerializer.Deserialize<FunctionCall>(
                jsonInstruction,
                _jsonSerializerOptions
            )!;
        }
        catch (Exception exception)
        {
            throw new Exception(
                errorHandler.GetFullErrorMessage(
                    $"Failed to deserialize function call. Response was:\n{jsonInstruction}"
                ),
                exception
            );
        }
    }

    private static object[] ConvertParametersForMethod(string[] rawParameters, MethodInfo method)
    {
        var methodParams = method.GetParameters();
        var convertedParameters = new object[rawParameters.Length];

        for (var i = 0; i < rawParameters.Length; i++)
        {
            var parameterType = methodParams[i].ParameterType;

            var converter = TypeDescriptor.GetConverter(parameterType);
            convertedParameters[i] = converter.ConvertFromString(rawParameters[i])!;
        }

        return convertedParameters;
    }
}
