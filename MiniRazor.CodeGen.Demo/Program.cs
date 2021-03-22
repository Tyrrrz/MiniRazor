using System;
using System.Threading.Tasks;

// Generated classes respect the @namespace directive in the template
using MiniRazor.CodeGen.Demo.Templates;

namespace MiniRazor.CodeGen.Demo
{
    public static class Program
    {
        public static async Task Main()
        {
            // Templates are automatically transpiled into C# classes.

            // You can use the static RenderAsync method to render
            // the template against a model.

            // Note that the model is statically typed and aligned
            // with the @inherits directive specified in the template.

            var result = await TemplateFoo.RenderAsync("John");

            Console.WriteLine("### TemplateFoo");
            Console.WriteLine(result);

            Console.WriteLine("### TemplateBar");
            await TemplateBar.RenderAsync(Console.Out, null);
        }
    }
}