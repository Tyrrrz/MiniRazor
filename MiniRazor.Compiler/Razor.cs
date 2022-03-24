using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MiniRazor.Exceptions;
using MiniRazor.Utils;
using MiniRazor.Utils.Extensions;

namespace MiniRazor;

/// <summary>
/// Methods to transpile and compile Razor templates.
/// </summary>
public static class Razor
{
    private static string? TryGetNamespace(string razorCode) =>
        Regex.Matches(razorCode, @"^\@namespace\s+(.+)$", RegexOptions.Multiline, TimeSpan.FromSeconds(1))
            .Cast<Match>()
            .LastOrDefault()?
            .Groups[1]
            .Value
            .Trim();

    /// <summary>
    /// Transpiles a Razor template into C# code.
    /// </summary>
    public static string Transpile(
        string source,
        string? accessModifier = null,
        Action<RazorProjectEngineBuilder>? configure = null)
    {
        // For some reason Razor engine ignores @namespace directive if
        // the file system is not configured properly.
        // So to work around it, we "parse" it ourselves.
        var actualNamespace =
            TryGetNamespace(source) ??
            "MiniRazor.GeneratedTemplates";

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

                    // Null access modifier resolved to internal by default in C#
                    if (!string.IsNullOrWhiteSpace(accessModifier))
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

    private static IReadOnlyList<MetadataReference> GetReferences(
        AssemblyLoadContext assemblyLoadContext,
        Assembly parentAssembly)
    {
        void PopulateTransitiveDependencies(Assembly assembly, ISet<AssemblyName> assemblyNames)
        {
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                // Avoid doing the same work twice
                if (!assemblyNames.Add(referencedAssemblyName))
                    continue;

                var referencedAssembly = assemblyLoadContext.TryLoadFromAssemblyName(referencedAssemblyName);
                if (referencedAssembly is not null)
                    PopulateTransitiveDependencies(referencedAssembly, assemblyNames);
            }
        }

        IEnumerable<MetadataReference?> EnumerateReferences()
        {
            // Implicit references

            yield return assemblyLoadContext
                .TryLoadFromAssemblyName(new AssemblyName("Microsoft.CSharp"))?
                .ToMetadataReference();

            yield return assemblyLoadContext
                .TryLoadFromAssemblyName(typeof(TemplateBase<>).Assembly.GetName())?
                .ToMetadataReference();

            if (!parentAssembly.IsDynamic)
            {
                yield return assemblyLoadContext
                    .TryLoadFromAssemblyName(parentAssembly.GetName())?
                    .ToMetadataReference();

                // References from parent assembly
                var transitiveDependencies = new HashSet<AssemblyName>(AssemblyNameEqualityComparer.Instance);
                PopulateTransitiveDependencies(parentAssembly, transitiveDependencies);

                foreach (var dependency in transitiveDependencies)
                {
                    yield return assemblyLoadContext
                        .TryLoadFromAssemblyName(dependency)?
                        .ToMetadataReference();
                }
            }
            
#if NETCOREAPP3_0_OR_GREATER
            if (parentAssembly.IsDynamic)
            {
                foreach (var assembly in assemblyLoadContext.Assemblies.Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location)))
                {
                    yield return assembly.ToMetadataReference();
                }
            }
#endif
        }

        return EnumerateReferences().WhereNotNull().Distinct().ToArray();
    }

    private static TemplateDescriptor Compile(
        string source,
        IReadOnlyList<MetadataReference> references,
        AssemblyLoadContext assemblyLoadContext)
    {
        var csharpCode = Transpile(source);
        var csharpDocumentAst = CSharpSyntaxTree.ParseText(csharpCode);

        var csharpDocumentCompilation = CSharpCompilation.Create(
            $"MiniRazor_Assembly_{Guid.NewGuid()}",
            new[] {csharpDocumentAst},
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var assemblyStream = new MemoryStream();
        var csharpDocumentCompilationResult = csharpDocumentCompilation.Emit(assemblyStream);

        if (!csharpDocumentCompilationResult.Success)
        {
            var errors = csharpDocumentCompilationResult
                .Diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToArray();

            throw new MiniRazorException(
                "Failed to compile template." +
                Environment.NewLine + Environment.NewLine +
                "Error(s):" +
                Environment.NewLine +
                errors.Select(m => "- " + m).JoinToString(Environment.NewLine) +
                Environment.NewLine + Environment.NewLine +
                "Generated source code:" +
                Environment.NewLine +
                csharpCode
            );
        }

        assemblyStream.Seek(0, SeekOrigin.Begin);
        var templateAssembly = assemblyLoadContext.LoadFromStream(assemblyStream);

        var templateType =
            templateAssembly.GetTypes().FirstOrDefault(t => t.Implements(typeof(ITemplate))) ??
            throw new InvalidOperationException("Could not locate compiled template in the generated assembly.");

        return new TemplateDescriptor(templateType);
    }

    /// <summary>
    /// Compiles a Razor template into executable code.
    /// Specified <see cref="AssemblyLoadContext"/> is used for assembly isolation.
    /// </summary>
    /// <remarks>
    /// Compiled resources are stored in memory and can only be released by unloading the context.
    /// </remarks>
    public static TemplateDescriptor Compile(string source, AssemblyLoadContext assemblyLoadContext) =>
        Compile(
            source,
            GetReferences(assemblyLoadContext, Assembly.GetCallingAssembly()),
            assemblyLoadContext
        );

    /// <summary>
    /// Compiles a Razor template into executable code.
    /// </summary>
    /// <remarks>
    /// Compiled resources are stored in memory and cannot be released.
    /// Use the overload that takes <see cref="AssemblyLoadContext"/> to specify a custom assembly context that can be unloaded.
    /// </remarks>
    public static TemplateDescriptor Compile(string source) =>
        Compile(
            source,
            GetReferences(AssemblyLoadContext.Default, Assembly.GetCallingAssembly()),
            AssemblyLoadContext.Default
        );
}