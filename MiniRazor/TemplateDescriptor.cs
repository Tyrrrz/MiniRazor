using System;
using System.IO;
using System.Threading.Tasks;
using MiniRazor.Internal.Extensions;

namespace MiniRazor
{
    /// <summary>
    /// Represents a compiled Razor template that can render output.
    /// </summary>
    public class TemplateDescriptor
    {
        private readonly Type _templateType;

        /// <summary>
        /// Initializes an instance of <see cref="TemplateDescriptor"/>.
        /// </summary>
        public TemplateDescriptor(Type templateType) => _templateType = templateType;

        private ITemplate CreateTemplateInstance() => (ITemplate) (
            Activator.CreateInstance(_templateType) ??
            throw new InvalidOperationException($"Could not instantiate compiled template of type '{_templateType}'.")
        );

        /// <summary>
        /// Renders the template using the specified writer.
        /// </summary>
        public async Task RenderAsync(TextWriter output, object? model = null)
        {
            var template = CreateTemplateInstance();

            var actualModel = model?.GetType().IsAnonymousType() == true
                ? model.ToExpando()
                : model;

            template.Model = actualModel;
            template.Output = output;

            await template.ExecuteAsync();
        }

        /// <summary>
        /// Renders the template to a string.
        /// </summary>
        public async Task<string> RenderAsync(object? model = null)
        {
            using var output = new StringWriter();
            await RenderAsync(output, model);

            return output.ToString();
        }
    }
}