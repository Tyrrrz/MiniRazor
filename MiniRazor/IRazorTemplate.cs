using System.Threading.Tasks;

namespace MiniRazor
{
    /// <summary>
    /// Razor template contract.
    /// </summary>
    public interface IRazorTemplate
    {
        /// <summary>
        /// Template model.
        /// </summary>
        dynamic? Model { get; set; }

        /// <summary>
        /// Write a raw literal string.
        /// </summary>
        void WriteLiteral(string? literal = null);

        /// <summary>
        /// Write an object.
        /// </summary>
        void Write(object? obj = null);

        /// <summary>
        /// Executes the template.
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Gets the rendered output
        /// </summary>
        string GetOutput();
    }
}