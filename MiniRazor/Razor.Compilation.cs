using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MiniRazor.Exceptions;
using MiniRazor.Internal;
using MiniRazor.Internal.Extensions;

namespace MiniRazor
{
    public static partial class Razor
    {
        private static readonly Cache<Assembly, IReadOnlyList<MetadataReference>> MetadataReferenceCache = new(20);

        private static IReadOnlyList<MetadataReference> GetReferences(Assembly parentAssembly)
        {
            IEnumerable<MetadataReference> EnumerateReferences()
            {
                // Implicit references
                yield return Assembly.Load("Microsoft.CSharp").ToMetadataReference();
                yield return typeof(TemplateDescriptor).Assembly.ToMetadataReference();
                yield return parentAssembly.ToMetadataReference();

                // References from parent assembly
                foreach (var dependency in parentAssembly.GetTransitiveDependencies())
                {
                    var dependencyAssembly = dependency.TryLoad();
                    if (dependencyAssembly != null)
                        yield return dependencyAssembly.ToMetadataReference();
                }
            }

            return MetadataReferenceCache.GetOrSet(
                parentAssembly,
                () => EnumerateReferences().Distinct().ToArray()
            );
        }

        private static TemplateDescriptor Compile(
            string source,
            IReadOnlyList<MetadataReference> references,
            AssemblyLoadContext assemblyLoadContext)
        {
            var csharpCode = ToCSharp(source);
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
                throw MiniRazorException.CompilationFailed(
                    csharpCode,
                    csharpDocumentCompilationResult.Diagnostics
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
            Compile(source, GetReferences(Assembly.GetCallingAssembly()), assemblyLoadContext);

        /// <summary>
        /// Compiles a Razor template into executable code.
        /// </summary>
        /// <remarks>
        /// Compiled resources are stored in memory and cannot be released.
        /// Use the overload that takes <see cref="AssemblyLoadContext"/> to specify a custom assembly context that can be unloaded.
        /// </remarks>
        public static TemplateDescriptor Compile(string source) =>
            Compile(source, GetReferences(Assembly.GetCallingAssembly()), AssemblyLoadContext.Default);
    }
}