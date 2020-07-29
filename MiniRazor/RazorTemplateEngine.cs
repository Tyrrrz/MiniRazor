using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MiniRazor.Exceptions;
using MiniRazor.Internal;
using MiniRazor.Internal.Extensions;

namespace MiniRazor
{
    /// <summary>
    /// Engine which can be used to compile Razor templates into executable code.
    /// </summary>
    public class RazorTemplateEngine
    {
        /// <summary>
        /// Name of the in-memory assembly where the templates are compiled.
        /// </summary>
        public string TemplateAssemblyName { get; }

        /// <summary>
        /// Root namespace, in which the compiled templates are located.
        /// </summary>
        public string RootNamespace { get; }

        /// <summary>
        /// Initializes an instance of <see cref="RazorTemplateEngine"/>.
        /// </summary>
        public RazorTemplateEngine(string templateAssemblyName, string? rootNamespace = null)
        {
            TemplateAssemblyName = templateAssemblyName;
            RootNamespace = rootNamespace ?? templateAssemblyName;
        }

        /// <summary>
        /// Initializes an instance of <see cref="RazorTemplateEngine"/>.
        /// </summary>
        public RazorTemplateEngine() : this("MiniRazor.Generated") { }

        private IReadOnlyList<MetadataReference> GetMetadataReferences()
        {
            var sourceAssembly = Assembly.GetEntryAssembly() ?? typeof(RazorTemplateEngine).Assembly;

            var transitiveAssemblies = sourceAssembly
                .GetTransitiveAssemblies()
                .Select(n => n.TryLoad())
                .Where(n => n != null);

            return transitiveAssemblies
                .Append(typeof(IRazorTemplate).Assembly)
                .Select(a => MetadataReference.CreateFromFile(a!.Location))
                .ToArray();
        }

        /// <summary>
        /// Compiles a Razor template from source code.
        /// </summary>
        /// <remarks>This method is CPU-intensive, so you may want to run it on a separate thread with <code>Task.Run(() => ...)</code></remarks>
        public RazorTemplateDescriptor Compile(string source)
        {
            const string templateTypeName = "MiniRazorTemplate";

            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                EmptyRazorProjectFileSystem.Instance,
                b => b
                    .SetNamespace(RootNamespace)
                    .SetBaseType(typeof(RazorTemplateBase).FullName)
                    .ConfigureClass((s, c) =>
                    {
                        // Internal instead of public so we can use internal types inside
                        c.Modifiers.Remove("public");
                        c.Modifiers.Add("internal");

                        c.ClassName = templateTypeName;
                    })
            );

            var sourceDocument = RazorSourceDocument.Create(
                source,
                $"{templateTypeName}.Generated.cs"
            );

            var codeDocument = engine.Process(
                sourceDocument,
                null,
                Array.Empty<RazorSourceDocument>(),
                Array.Empty<TagHelperDescriptor>()
            );

            var csharpDocument = codeDocument.GetCSharpDocument();
            var csharpDocumentAst = CSharpSyntaxTree.ParseText(csharpDocument.GeneratedCode);
            var csharpDocumentCompilation = CSharpCompilation.Create(
                TemplateAssemblyName,
                new[] {csharpDocumentAst},
                GetMetadataReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var assemblyStream = new MemoryStream();
            var csharpDocumentCompilationResult = csharpDocumentCompilation.Emit(assemblyStream);

            if (!csharpDocumentCompilationResult.Success)
                throw RazorCompilationException.FromDiagnostics(csharpDocument.GeneratedCode, csharpDocumentCompilationResult.Diagnostics);

            var assembly = Assembly.Load(assemblyStream.ToArray());

            var templateType =
                assembly.GetTypes().SingleOrDefault(t => t.Name.Equals(templateTypeName, StringComparison.Ordinal)) ??
                throw new InvalidOperationException("Could not locate compiled template in the generated assembly.");

            return new RazorTemplateDescriptor(templateType);
        }
    }
}