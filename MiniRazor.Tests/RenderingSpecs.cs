using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MiniRazor.Tests.Models;
using Xunit;

namespace MiniRazor.Tests
{
    public class RenderingSpecs
    {
        [Fact]
        public async Task I_can_render_a_template_with_a_model()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Model.Foo & @Model.Number");

            // Act
            var result = await template.RenderAsync(new TestModel("bar", 42));

            // Assert
            result.Should().Be("bar & 42");
        }

        [Fact]
        public async Task I_can_render_a_template_with_an_internal_model()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Model.Foo", "FriendlyAssemblyName");

            // Act
            var result = await template.RenderAsync(new TestInternalModel("bar"));

            // Assert
            result.Should().Be("bar");
        }

        [Fact]
        public async Task I_can_render_a_template_which_calls_methods_on_a_model()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Model.GetSum()");

            // Act
            var result = await template.RenderAsync(new TestModelWithMethod(2, 5));

            // Assert
            result.Should().Be("7");
        }

        [Fact]
        public async Task I_can_render_a_template_which_calls_async_methods_on_a_model()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@await Model.GetSumAsync()");

            // Act
            var result = await template.RenderAsync(new TestModelWithAsyncMethod(2, 5));

            // Assert
            result.Should().Be("7");
        }

        [Fact]
        public async Task I_can_render_a_template_which_calls_a_locally_defined_method()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@{ int GetNumber() => 42; }@GetNumber()");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("42");
        }

        [Fact]
        public async Task I_can_render_a_template_with_an_anonymous_model()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("Hello, @Model.Foo!");

            // Act
            var result = await template.RenderAsync(new {Foo = "World"});

            // Assert
            result.Should().Be("Hello, World!");
        }

        [Fact]
        public async Task I_can_render_a_template_with_an_anonymous_model_that_contains_nested_anonymous_objects()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Model.Foo, @Model.Bar.X, @Model.Bar.Y");

            // Act
            var result = await template.RenderAsync(new
            {
                Foo = "xxx",
                Bar = new
                {
                    X = 42,
                    Y = 13
                }
            });

            // Assert
            result.Should().Be("xxx, 42, 13");
        }

        [Fact]
        public async Task I_can_render_a_template_multiple_times_without_recompiling()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("Hello, @Model.Foo!");

            // Act
            var results = await Task.WhenAll(
                Enumerable.Range(0, 5).Select(async _ =>
                    await template.RenderAsync(new {Foo = "World"}))
            );

            // Assert
            results.Distinct().Should().ContainSingle();
        }
    }
}