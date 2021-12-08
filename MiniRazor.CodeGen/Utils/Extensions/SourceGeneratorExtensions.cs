using Microsoft.CodeAnalysis.Diagnostics;

namespace MiniRazor.CodeGen.Utils.Extensions;

internal static class SourceGeneratorExtensions
{
    public static string? TryGetValue(this AnalyzerConfigOptions options, string key) =>
        options.TryGetValue(key, out var value) ? value : null;

    public static string? TryGetAdditionalFileMetadataValue(this AnalyzerConfigOptions options, string propertyName) =>
        options.TryGetValue($"build_metadata.AdditionalFiles.{propertyName}");
}