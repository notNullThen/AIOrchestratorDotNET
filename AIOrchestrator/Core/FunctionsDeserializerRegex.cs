namespace AIOrchestrator.Core;

using System.Text.RegularExpressions;

// Ignores whitespaces
internal static partial class FunctionsDeserializerRegex
{
    [GeneratedRegex(@"\{\s*""function""\s*:\s*""", RegexOptions.IgnoreCase)]
    internal static partial Regex FunctionCallStart();

    [GeneratedRegex(@"\]\s*\}")]
    internal static partial Regex FunctionCallEnd();
}
