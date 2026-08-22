namespace AIOrchestrator.Core;

using System.Text.RegularExpressions;

// Ignores whitespaces
internal static partial class FunctionsDeserializerRegex
{
    [GeneratedRegex(@"\{\s*""function""\s*:\s*""", RegexOptions.IgnoreCase)]
    internal static partial Regex FunctionStart();

    [GeneratedRegex(@"""\s*\}")]
    internal static partial Regex FunctionEnd();
}
