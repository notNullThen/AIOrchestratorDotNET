namespace AIOrchestrator.Utilities;

using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using AIOrchestrator.Core;
using Core.Types;

internal class MethodInvoker(ErrorHandler errorHandler)
{
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

    private static object[] ConvertParametersForMethod(string[] rawParameters, MethodInfo method)
    {
        var methodParams = method.GetParameters();

        if (rawParameters.Length > methodParams.Length)
        {
            throw new TargetParameterCountException(
                $"Method {method.Name}() received more parameters than expected."
            );
        }

        var convertedParameters = new object[methodParams.Length];

        for (var i = 0; i < methodParams.Length; i++)
        {
            var parameterType = methodParams[i].ParameterType;
            var rawParameter = i < rawParameters.Length ? rawParameters[i] : string.Empty;

            var converter = TypeDescriptor.GetConverter(parameterType);
            convertedParameters[i] = converter.ConvertFromString(rawParameter)!;
        }

        return convertedParameters;
    }
}
