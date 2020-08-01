using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MiniRazor.Exceptions;
using MiniRazor.Tests.Models;
using Xunit;

namespace MiniRazor.Tests
{
    public class TemplateSpecs
    {
        [Fact]
        public void I_can_compile_a_template_and_get_an_error_if_it_is_invalid()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();

            // Act & assert
            Assert.Throws<MiniRazorCompilationException>(() =>
                engine.Compile("@Xyz")
            );
        }

        [Fact]
        public async Task I_can_compile_multiple_templates_using_the_same_engine()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();

            var template1 = engine.Compile("Hello, @Model.Foo!");
            var template2 = engine.Compile("Goodbye, @Model.Bar...");

            // Act
            var result1 = await template1.RenderAsync(new {Foo = "World"});
            var result2 = await template2.RenderAsync(new {Bar = "Meagerness"});

            // Assert
            result1.Should().Be("Hello, World!");
            result2.Should().Be("Goodbye, Meagerness...");
        }

        [Fact]
        public async Task I_can_compile_templates_using_different_engines()
        {
            // Arrange
            using var engine1 = new MiniRazorTemplateEngine();
            using var engine2 = new MiniRazorTemplateEngine();

            var template1 = engine1.Compile("Hello, @Model.Foo!");
            var template2 = engine2.Compile("Goodbye, @Model.Bar...");

            // Act
            var result1 = await template1.RenderAsync(new {Foo = "World"});
            var result2 = await template2.RenderAsync(new {Bar = "Meagerness"});

            // Assert
            result1.Should().Be("Hello, World!");
            result2.Should().Be("Goodbye, Meagerness...");
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

        [Fact]
        public async Task I_can_render_a_template_with_an_anonymous_object()
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
        public async Task I_can_render_a_template_with_an_anonymous_object_that_contains_nested_anonymous_objects()
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
        public async Task I_can_render_a_template_which_contains_HTML_tokens()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("<div id=\"zzz\">@Model.Foo</div>");

            // Act
            var result = await template.RenderAsync(new {Foo = "bar"});

            // Assert
            result.Should().Be("<div id=\"zzz\">bar</div>");
        }

        [Fact]
        public async Task I_can_render_text_inside_of_an_HTML_attribute()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("<div class=\"xyz @Model.Foo\">fff</div>");

            // Act
            var result = await template.RenderAsync(new {Foo = "bar"});

            // Assert
            result.Should().Be("<div class=\"xyz bar\">fff</div>");
        }

        [Fact]
        public async Task I_can_render_text_that_contains_HTML_tokens_and_have_them_automatically_encoded()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@(\"<div id='foo'>bar</div>\")");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("&lt;div id=&#39;foo&#39;&gt;bar&lt;/div&gt;");
        }

        [Fact]
        public async Task I_can_render_text_that_contains_HTML_tokens_and_manually_opt_out_of_encoding()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Raw(\"<div id='foo'>bar</div>\")");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("<div id='foo'>bar</div>");
        }
    }
}