using System.Runtime.Loader;
using System.Threading.Tasks;
using FluentAssertions;
using MiniRazor.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace MiniRazor.Compiler.Tests
{
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
    }
}