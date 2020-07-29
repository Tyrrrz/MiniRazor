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
        /// Initializes an instance of <see cref="RazorTemplateEngine"/>.
        /// </summary>
        public RazorTemplateEngine(string templateAssemblyName) =>
            TemplateAssemblyName = templateAssemblyName;

        /// <summary>
        /// Initializes an instance of <see cref="RazorTemplateEngine"/>.
        /// </summary>
        public RazorTemplateEngine() : this("MiniRazor.InMemoryAssembly") { }

        private IReadOnlyList<MetadataReference> GetMetadataReferences()
        {
            var sourceAssembly = Assembly.GetEntryAssembly() ?? typeof(RazorTemplateEngine).Assembly;

            var transitiveAssemblies = sourceAssembly.GetTransitiveAssemblies().Distinct().Select(n => n.Load());

            return transitiveAssemblies
                .Append(typeof(IRazorTemplate).Assembly)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToArray();
        }

        /// <summary>
        /// Compiles a Razor template from source code.
        /// </summary>
        public RazorTemplateDescriptor Compile(string source)
        {
            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                EmptyRazorProjectFileSystem.Instance,
                b => b.SetBaseType(typeof(RazorTemplateBase).FullName)
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
                throw RazorCompilationException.FromDiagnostics(csharpDocumentCompilationResult.Diagnostics);

            var assembly = Assembly.Load(assemblyStream.ToArray());

            var templateType =
                assembly.ExportedTypes.SingleOrDefault(t => t.Implements(typeof(IRazorTemplate))) ??
                throw new InvalidOperationException("Could not locate compiled template in the generated assembly.");

            return new RazorTemplateDescriptor(templateType);
        }
    }
}