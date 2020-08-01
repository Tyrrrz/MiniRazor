using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MiniRazor.Tests
{
    public class HtmlEncodingSpecs
    {
        [Fact]
        public async Task I_can_render_a_template_that_contains_HTML_tokens_and_they_do_not_get_encoded()
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
        public async Task I_can_render_a_template_that_puts_text_containing_HTML_tokens_and_it_gets_encoded()
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
        public async Task I_can_render_a_template_that_puts_text_containing_HTML_tokens_and_disable_encoding()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("@Raw(\"<div id='foo'>bar</div>\")");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("<div id='foo'>bar</div>");
        }

        [Fact]
        public async Task I_can_render_a_template_that_puts_text_inside_of_an_HTML_attribute()
        {
            // Arrange
            using var engine = new MiniRazorTemplateEngine();
            var template = engine.Compile("<div class=\"xyz @Model.Foo\">fff</div>");

            // Act
            var result = await template.RenderAsync(new {Foo = "bar"});

            // Assert
            result.Should().Be("<div class=\"xyz bar\">fff</div>");
        }
    }
}