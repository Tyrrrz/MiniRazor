using System.IO;
using System.Threading.Tasks;

namespace MiniRazor
{
    /// <summary>
    /// Non-generic handle for <see cref="TemplateBase{TModel}"/> to simplify usage from reflection.
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Template output.
        /// </summary>
        TextWriter? Output { get; set; }

        /// <summary>
        /// Template model.
        /// </summary>
        object? Model { get; set; }

        /// <summary>
        /// Executes the template.
        /// </summary>
        Task ExecuteAsync();
    }
}