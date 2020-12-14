using System.Runtime.Loader;
using System.Threading.Tasks;
using FluentAssertions;
using MiniRazor.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace MiniRazor.Tests
{
    public class CompilationSpecs
    {
        private readonly ITestOutputHelper _testOutput;

        public CompilationSpecs(ITestOutputHelper testOutput) => _testOutput = testOutput;

        [Fact]
        public void Compiling_a_template_throws_an_exception_if_it_is_invalid()
        {
            // Act & assert
            var ex = Assert.Throws<MiniRazorException>(() =>
                Razor.Compile("@Xyz")
            );

            _testOutput.WriteLine(ex.Message);
        }

        [Fact]
        public async Task Multiple_templates_can_be_compiled_independently()
        {
            // Act
            var template1 = Razor.Compile("Hello, @Model.Foo!");
            var template2 = Razor.Compile("Goodbye, @Model.Bar...");

            // Assert
            var result1 = await template1.RenderAsync(new {Foo = "World"});
            var result2 = await template2.RenderAsync(new {Bar = "Meagerness"});

            result1.Should().Be("Hello, World!");
            result2.Should().Be("Goodbye, Meagerness...");
        }

#if NET5_0
        [Fact]
        public async Task Template_can_be_compiled_with_a_custom_assembly_load_context()
        {
            // Arrange
            var assemblyLoadContext = new AssemblyLoadContext(null, true);

            // Act
            var template = Razor.Compile("Hello world!", assemblyLoadContext);

            // Assert
            var result = await template.RenderAsync();
            result.Should().Be("Hello world!");
        }
#endif
    }
}