using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
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
    
    [Fact]
    public void Template_can_be_compiled_from_a_dynamic_assembly()
    {
        var pathToMiniRazorAssembly = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "MiniRazor.Compiler.dll");
        
        //language=C#
        var sourceCode = $@"using System;
using System.Runtime.Loader;
using System.Reflection;

public class Program
{{
    public static int Main()
    {{
        var useAssemblyLoadContextToInferReferences = true;
        
        var miniRazorAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(""{pathToMiniRazorAssembly}"");
        var compilationOptionsType = miniRazorAssembly.GetType(""MiniRazor.RazorCompileOptions"", true);
        object optionsInstance = Activator.CreateInstance(compilationOptionsType);
        PropertyInfo inferOptionProperty = compilationOptionsType.GetProperty(""InferReferencesFromAssemblyLoadContext"");
        inferOptionProperty.SetValue(optionsInstance, useAssemblyLoadContextToInferReferences, null);

        var template = miniRazorAssembly.GetType(""MiniRazor.Razor"").GetMethod(""Compile"", new [] {{typeof(string), compilationOptionsType}}).Invoke(null, new object[] {{ ""Hello world!"", optionsInstance}});
        return 0;
    }}
}}
";
        
        var ast = SyntaxFactory.ParseSyntaxTree(
            SourceText.From(sourceCode),
            CSharpParseOptions.Default
        );
        
        // Compile the code to IL
        var compilation = CSharpCompilation.Create(
            "MiniRazorCompilerTests_DynamicAssembly_" + Guid.NewGuid(),
            new[] {ast},
            ReferenceAssemblies.Net60);
                //.Append(MetadataReference.CreateFromFile(typeof(Razor).Assembly.Location))
        

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

        // Emit the code to an in-memory buffer
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
        
        // Load the generated assembly
        var generatedAssembly = Assembly.Load(buffer.ToArray());
        
        // Execute the entry point method
        var result = (int) generatedAssembly.EntryPoint!.Invoke(null, null)!;
        
        Assert.Equal(0, result);
    }
}