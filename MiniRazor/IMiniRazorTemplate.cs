using System.Threading.Tasks;

namespace MiniRazor
{
    /// <summary>
    /// Razor template contract.
    /// </summary>
    public interface IMiniRazorTemplate
    {
        /// <summary>
        /// Template model.
        /// </summary>
        dynamic? Model { get; set; }

        /// <summary>
        /// Writes a raw literal string.
        /// </summary>
        void WriteLiteral(string? literal = null);

        /// <summary>
        /// Writes an object.
        /// </summary>
        void Write(object? obj = null);

        /// <summary>
        /// Begins writing attribute.
        /// </summary>
        void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);

        /// <summary>
        /// Writes attribute value.
        /// </summary>
        void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral);

        /// <summary>
        /// Ends writing attribute.
        /// </summary>
        void EndWriteAttribute();

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