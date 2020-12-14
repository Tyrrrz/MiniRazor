using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MiniRazor.Tests
{
    public class EncodingSpecs
    {
        [Fact]
        public async Task Template_literals_are_always_rendered_without_encoding()
        {
            // Arrange
            var template = Razor.Compile("<div id=\"zzz\">@Model.Foo</div>");

            // Act
            var result = await template.RenderAsync(new {Foo = "bar"});

            // Assert
            result.Should().Be("<div id=\"zzz\">bar</div>");
        }

        [Fact]
        public async Task Template_literals_in_attributes_are_always_rendered_without_encoding()
        {
            // Arrange
            var template = Razor.Compile("<div class=\"xyz @Model.Foo\">fff</div>");

            // Act
            var result = await template.RenderAsync(new {Foo = "bar"});

            // Assert
            result.Should().Be("<div class=\"xyz bar\">fff</div>");
        }

        [Fact]
        public async Task Evaluated_expressions_are_rendered_with_encoding_by_default()
        {
            // Arrange
            var template = Razor.Compile("@(\"<div id='foo'>bar</div>\")");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("&lt;div id=&#39;foo&#39;&gt;bar&lt;/div&gt;");
        }

        [Fact]
        public async Task Evaluated_expressions_can_be_rendered_without_encoding()
        {
            // Arrange
            var template = Razor.Compile("@Raw(\"<div id='foo'>bar</div>\")");

            // Act
            var result = await template.RenderAsync();

            // Assert
            result.Should().Be("<div id='foo'>bar</div>");
        }
    }
}