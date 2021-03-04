using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language;

namespace MiniRazor
{
    /// <summary>
    /// Utilities for working with Razor templates.
    /// </summary>
    public static partial class Razor
    {
        private static string? TryGetNamespace(string razorCode) =>
            Regex.Matches(razorCode, @"^\@namespace\s+(.+)$", RegexOptions.Multiline, TimeSpan.FromSeconds(1))
                .Cast<Match>()
                .LastOrDefault()?
                .Groups[1]
                .Value
                .Trim();

        // Consumed from CodeGen
        internal static string ToCSharp(string source, string accessModifier, Action<RazorProjectEngineBuilder>? configure = null)
        {
            // For some reason Razor engine ignores @namespace directive if
            // the file system is not configured properly.
            // So to work around it, we "parse" it ourselves.
            var actualNamespace = TryGetNamespace(source) ?? "MiniRazor.GeneratedTemplates";

            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                EmptyRazorProjectFileSystem.Instance,
                options =>
                {
                    options.SetNamespace(actualNamespace);
                    options.SetBaseType("MiniRazor.TemplateBase<dynamic>");

                    options.ConfigureClass((_, node) =>
                    {
                        node.Modifiers.Clear();

                        node.Modifiers.Add(accessModifier);

                        // Partial to allow extension
                        node.Modifiers.Add("partial");
                    });

                    configure?.Invoke(options);
                }
            );

            var sourceDocument = RazorSourceDocument.Create(
                source,
                $"MiniRazor_GeneratedTemplate_{Guid.NewGuid()}.cs"
            );

            var codeDocument = engine.Process(
                sourceDocument,
                null,
                Array.Empty<RazorSourceDocument>(),
                Array.Empty<TagHelperDescriptor>()
            );

            return codeDocument.GetCSharpDocument().GeneratedCode;
        }
    }
}