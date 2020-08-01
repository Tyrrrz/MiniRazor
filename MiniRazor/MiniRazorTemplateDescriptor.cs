using System;
using System.Threading.Tasks;
using MiniRazor.Internal.Extensions;

namespace MiniRazor
{
    /// <summary>
    /// Compiled Razor template which can be used to render output.
    /// </summary>
    public class MiniRazorTemplateDescriptor
    {
        private readonly Type _templateType;

        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorTemplateDescriptor"/>.
        /// </summary>
        public MiniRazorTemplateDescriptor(Type templateType) =>
            _templateType = templateType;

        private MiniRazorTemplateBase ActivateTemplate() => (MiniRazorTemplateBase)
            (Activator.CreateInstance(_templateType) ??
             throw new InvalidOperationException($"Could not instantiate template of type '{_templateType}'."));

        /// <summary>
        /// Renders the template with the specified model.
        /// </summary>
        public async Task<string> RenderAsync(object? model = null)
        {
            var template = ActivateTemplate();

            template.SetModel(
                model?.GetType().IsAnonymousType() == true
                    ? model?.ToExpando()
                    : model
            );

            await template.ExecuteAsync();

            return template.GetOutput();
        }
    }
}