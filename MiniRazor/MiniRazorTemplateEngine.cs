using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
    public partial class MiniRazorTemplateEngine : IDisposable
    {
        private readonly AssemblyLoadContext _assemblyLoadContext = new AssemblyLoadContext("MiniRazorGeneratedALC", true);
        private readonly Lazy<IReadOnlyList<MetadataReference>> _metadataReferencesLazy;

        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorTemplateEngine"/>.
        /// </summary>
        public MiniRazorTemplateEngine(Assembly parentAssembly)
        {
            _metadataReferencesLazy = new Lazy<IReadOnlyList<MetadataReference>>(() => GetMetadataReferences(parentAssembly));
        }

        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorTemplateEngine"/>.
        /// </summary>
        public MiniRazorTemplateEngine()
            : this(Assembly.GetCallingAssembly())
        {
        }

        private RazorCodeDocument ProcessRazorCode(string source, string rootNamespace, string typeName)
        {
            var internalEngine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                EmptyRazorProjectFileSystem.Instance,
                b => b
                    .SetNamespace(rootNamespace)
                    .SetBaseType(typeof(MiniRazorTemplateBase).FullName)
                    .ConfigureClass((s, c) =>
                    {
                        // Internal instead of public so we can reference internal types inside
                        c.Modifiers.Remove("public");
                        c.Modifiers.Add("internal");

                        c.ClassName = typeName;
                    })
            );

            var sourceDocument = RazorSourceDocument.Create(
                source,
                $"{typeName}.Generated.cs"
            );

            return internalEngine.Process(
                sourceDocument,
                null,
                Array.Empty<RazorSourceDocument>(),
                Array.Empty<TagHelperDescriptor>()
            );
        }

        private Assembly CompileRazorCode(string assemblyName, RazorCodeDocument codeDocument)
        {
            var csharpDocument = codeDocument.GetCSharpDocument();
            var csharpDocumentAst = CSharpSyntaxTree.ParseText(csharpDocument.GeneratedCode);

            var csharpDocumentCompilation = CSharpCompilation.Create(
                assemblyName,
                new[] {csharpDocumentAst},
                _metadataReferencesLazy.Value,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var assemblyStream = new MemoryStream();
            var csharpDocumentCompilationResult = csharpDocumentCompilation.Emit(assemblyStream);

            if (!csharpDocumentCompilationResult.Success)
            {
                throw MiniRazorException.CompilationFailed(
                    csharpDocument.GeneratedCode,
                    csharpDocumentCompilationResult.Diagnostics
                );
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);
            return _assemblyLoadContext.LoadFromStream(assemblyStream);
        }

        /// <summary>
        /// Compiles a Razor template from source code.
        /// </summary>
        /// <remarks>This method is CPU-intensive, so you may want to run it on a separate thread with <code>Task.Run(() => ...)</code></remarks>
        public MiniRazorTemplateDescriptor Compile(string source, string assemblyName, string rootNamespace)
        {
            const string templateTypeName = "MiniRazorTemplate";

            var templateCode = ProcessRazorCode(source, rootNamespace, templateTypeName);
            var templateAssembly = CompileRazorCode(assemblyName, templateCode);

            var templateType =
                templateAssembly.GetTypes().SingleOrDefault(t => t.Name.Equals(templateTypeName, StringComparison.Ordinal)) ??
                throw new InvalidOperationException("Could not locate compiled template in the generated assembly.");

            return new MiniRazorTemplateDescriptor(templateType);
        }

        /// <summary>
        /// Compiles a Razor template from source code.
        /// </summary>
        /// <remarks>This method is CPU-intensive, so you may want to run it on a separate thread with <code>Task.Run(() => ...)</code></remarks>
        public MiniRazorTemplateDescriptor Compile(string source, string assemblyName) =>
            Compile(source, assemblyName, assemblyName);

        /// <summary>
        /// Compiles a Razor template from source code.
        /// </summary>
        /// <remarks>This method is CPU-intensive, so you may want to run it on a separate thread with <code>Task.Run(() => ...)</code></remarks>
        public MiniRazorTemplateDescriptor Compile(string source) =>
            Compile(source, $"MiniRazorTemplateAssembly_{GenerateSalt()}");

        /// <inheritdoc />
        public void Dispose() => _assemblyLoadContext.Unload();
    }

    public partial class MiniRazorTemplateEngine
    {
        private static string GenerateSalt() => Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);

        private static IReadOnlyList<MetadataReference> GetMetadataReferences(Assembly parentAssembly)
        {
            var implicitAssemblies = new[]
            {
                parentAssembly,
                Assembly.Load("Microsoft.CSharp"),
                typeof(MiniRazorTemplateEngine).Assembly
            };

            var dependencyAssemblies = parentAssembly
                .GetTransitiveAssemblies()
                .Select(n => n.TryLoad())
                .Where(n => n != null);

            return implicitAssemblies.Concat(dependencyAssemblies)
                .Distinct()
                .Select(a => MetadataReference.CreateFromFile(a!.Location))
                .ToArray();
        }
    }
}