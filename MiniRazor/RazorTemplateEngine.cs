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
        public RazorTemplateEngine(string templateAssemblyName, string rootNamespace)
        {
            TemplateAssemblyName = templateAssemblyName;
            RootNamespace = rootNamespace;
        }

        /// <summary>
        /// Initializes an instance of <see cref="RazorTemplateEngine"/>.
        /// </summary>
        public RazorTemplateEngine() : this("MiniRazor.$Dynamic", "MiniRazor.$Dynamic") { }

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
            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                EmptyRazorProjectFileSystem.Instance,
                b => b
                    .SetRootNamespace(RootNamespace)
                    .SetBaseType(typeof(RazorTemplateBase).FullName)
            );

            var sourceDocument = RazorSourceDocument.Create(
                source,
                "RazorTemplate.Generated.cs"
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
                assembly.ExportedTypes.SingleOrDefault(t => t.Implements(typeof(IRazorTemplate))) ??
                throw new InvalidOperationException("Could not locate compiled template in the generated assembly.");

            return new RazorTemplateDescriptor(templateType);
        }
    }
}