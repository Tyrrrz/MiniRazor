using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MiniRazor.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace MiniRazor.Compiler.Tests;

public class CompilationSpecs
{
    private readonly ITestOutputHelper _testOutput;

    public CompilationSpecs(ITestOutputHelper testOutput) => _testOutput = testOutput;

    [Fact]
    public async Task Template_can_be_compiled()
    {
        // Act
        var template = Razor.Compile("Hello world!");

        // Assert
        var result = await template.RenderAsync(null);
        result.Should().Be("Hello world!");
    }

    [Fact]
    public async Task Template_can_be_compiled_with_a_custom_assembly_load_context()
    {
        // Arrange
        var assemblyLoadContext = new AssemblyLoadContext(null, true);

        // Act
        var template = Razor.Compile("Hello world!", assemblyLoadContext);

        // Assert
        var result = await template.RenderAsync(null);
        result.Should().Be("Hello world!");
    }

    [Fact]
    public void Template_can_be_compiled_with_dynamically_loaded_assembly()
    {
        // Arrange
        // language=cs
        var sourceCode = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using MiniRazor;

public class Program
{{
    public static async Task<int> Main()
    {{
        var template = Razor.Compile(""Hello world!"");
        var result = await template.RenderAsync(null);
        return result.Length;
    }}
}}
";

        var ast = SyntaxFactory.ParseSyntaxTree(
            SourceText.From(sourceCode),
            CSharpParseOptions.Default
        );

        var compilation = CSharpCompilation.Create(
            $"TestAssembly_{Guid.NewGuid()}",
            new[] { ast },
            ReferenceAssemblies.Net60
                .Append(MetadataReference.CreateFromFile(typeof(Razor).Assembly.Location))
        );

        var compilationErrors = compilation
            .GetDiagnostics()
            .Where(d => d.Severity >= DiagnosticSeverity.Error)
            .ToArray();

        if (compilationErrors.Any())
        {
            throw new InvalidOperationException(
                "Failed to compile code." +
                Environment.NewLine +
                string.Join(Environment.NewLine, compilationErrors.Select(e => e.ToString()))
            );
        }

        using var buffer = new MemoryStream();
        var emit = compilation.Emit(buffer);

        var emitErrors = emit
            .Diagnostics
            .Where(d => d.Severity >= DiagnosticSeverity.Error)
            .ToArray();

        if (emitErrors.Any())
        {
            throw new InvalidOperationException(
                "Failed to emit code." +
                Environment.NewLine +
                string.Join(Environment.NewLine, emitErrors.Select(e => e.ToString()))
            );
        }

        var generatedAssembly = Assembly.Load(buffer.ToArray());

        // Act
        var result = generatedAssembly.EntryPoint!.Invoke(null, null);

        // Assert
        result.Should().Be(12);
    }

    [Fact]
    public async Task Multiple_templates_can_be_compiled_independently()
    {
        // Act
        var template1 = Razor.Compile("Hello world!");
        var template2 = Razor.Compile("Goodbye world...");

        // Assert
        var result1 = await template1.RenderAsync(null);
        var result2 = await template2.RenderAsync(null);

        result1.Should().Be("Hello world!");
        result2.Should().Be("Goodbye world...");
    }

    [Fact]
    public void Template_compilation_fails_if_the_template_is_invalid()
    {
        // Act & assert
        var ex = Assert.Throws<MiniRazorException>(() =>
            Razor.Compile("Hello @Xyz @Foo!")
        );

        _testOutput.WriteLine(ex.Message);
    }
}