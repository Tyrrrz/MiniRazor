using System;
using System.Threading.Tasks;
using MiniRazor.Internal.Extensions;

namespace MiniRazor
{
    /// <summary>
    /// Compiled Razor template which can be used to render output.
    /// </summary>
    public class RazorTemplateDescriptor
    {
        private readonly Type _templateType;

        /// <summary>
        /// Initializes an instance of <see cref="RazorTemplateDescriptor"/>.
        /// </summary>
        public RazorTemplateDescriptor(Type templateType) =>
            _templateType = templateType;

        private IRazorTemplate ActivateTemplate() => (IRazorTemplate)
            (Activator.CreateInstance(_templateType) ??
             throw new InvalidOperationException($"Could not instantiate template of type '{_templateType}'."));

        /// <summary>
        /// Renders the template with the specified model.
        /// </summary>
        public async Task<string> RenderAsync(object? model = null)
        {
            var template = ActivateTemplate();

            template.Model = model?.GetType().IsAnonymousType() == true
                ? model?.ToExpando()
                : model;

            await template.ExecuteAsync();

            return template.GetOutput();
        }
    }
}