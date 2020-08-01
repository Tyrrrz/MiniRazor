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
        public void I_can_compile_a_template_and_get_an_error_if_it_is_invalid()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();

            // Act & assert
            var ex = Assert.Throws<MiniRazorException>(() =>
                engine.Compile("@Xyz")
            );

            _testOutput.WriteLine(ex.Message);
        }

        [Fact]
        public async Task I_can_compile_multiple_templates_using_the_same_engine()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();

            // Act
            var template1 = engine.Compile("Hello, @Model.Foo!");
            var template2 = engine.Compile("Goodbye, @Model.Bar...");

            var result1 = await template1.RenderAsync(new {Foo = "World"});
            var result2 = await template2.RenderAsync(new {Bar = "Meagerness"});

            // Assert
            result1.Should().Be("Hello, World!");
            result2.Should().Be("Goodbye, Meagerness...");
        }

        [Fact]
        public async Task I_can_compile_multiple_templates_using_different_engines()
        {
            // Arrange
            using var engine1 = new MiniRazorTemplateEngine();
            using var engine2 = new MiniRazorTemplateEngine();

            // Act
            var template1 = engine1.Compile("Hello, @Model.Foo!");
            var template2 = engine2.Compile("Goodbye, @Model.Bar...");

            var result1 = await template1.RenderAsync(new {Foo = "World"});
            var result2 = await template2.RenderAsync(new {Bar = "Meagerness"});

            // Assert
            result1.Should().Be("Hello, World!");
            result2.Should().Be("Goodbye, Meagerness...");
        }
    }
}