using System.Threading.Tasks;
using FluentAssertions;
using MiniRazor.Tests.Models;
using Xunit;

using MiniRazor;

namespace MiniRazor.Tests
{
    public class RenderingSpecs
    {
        [Fact]
        public async Task Template_can_reference_a_model()
        {
            // Arrange
            var template = Razor.Compile("@Model.Foo & @Model.Number");

            // Act
            var result = await template.RenderAsync(new TestModel("bar", 42));

            // Assert
            result.Should().Be("bar & 42");
        }

        [Fact]
        public async Task Template_can_call_methods_on_a_model()
        {
            // Arrange
            var template = Razor.Compile("@Model.GetSum()");

            // Act
            var result = await template.RenderAsync(new TestModelWithMethod(2, 5));

            // Assert
            result.Should().Be("7");
        }

        [Fact]
        public async Task Template_can_call_asynchronous_methods_on_a_model()
        {
            // Arrange
            var template = Razor.Compile("@await Model.GetSumAsync()");

            // Act
            var result = await template.RenderAsync(new TestModelWithAsyncMethod(2, 5));

            // Assert
            result.Should().Be("7");
        }

        [Fact]
        public async Task Template_can_define_and_execute_local_code()
        {
            // Arrange
            var template = Razor.Compile("@{ int GetNumber() => 42; }@GetNumber()");

            // Act
            var result = await template.RenderAsync(null);

            // Assert
            result.Should().Be("42");
        }

        [Fact]
        public async Task Template_can_be_rendered_with_an_anonymous_model()
        {
            // Arrange
            var template = Razor.Compile("Hello, @Model.Foo!");

            // Act
            var result = await template.RenderAsync(new {Foo = "World"});

            // Assert
            result.Should().Be("Hello, World!");
        }

        [Fact]
        public async Task Template_can_be_rendered_with_a_recursive_anonymous_model()
        {
            // Arrange
            var template = Razor.Compile("@Model.Foo, @Model.Bar.X, @Model.Bar.Y");

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
    }
}