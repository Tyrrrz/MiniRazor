using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniRazor.Primitives;

namespace MiniRazor
{
    /// <summary>
    /// Base implementation of <see cref="IRazorTemplate"/>.
    /// </summary>
    public abstract class RazorTemplateBase : IRazorTemplate
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        /// <inheritdoc />
        public dynamic? Model { get; set; }

        /// <inheritdoc />
        public void WriteLiteral(string? literal = null)
        {
            if (literal != null)
                _buffer.Append(literal);
        }

        private void Write(string? str = null)
        {
            if (str != null)
                WriteLiteral(WebUtility.HtmlEncode(str));
        }

        /// <inheritdoc />
        public void Write(object? obj = null)
        {
            switch (obj)
            {
                case string s:
                    Write(s);
                    break;

                case RawString s:
                    WriteLiteral(s.Value);
                    break;

                default:
                    Write(obj?.ToString());
                    break;
            }
        }

        /// <summary>
        /// Wraps a string into a container that instructs the renderer to avoid encoding.
        /// </summary>
        public RawString Raw(string value) => new RawString(value);

        /// <inheritdoc />
        public abstract Task ExecuteAsync();

        /// <inheritdoc />
        public string GetOutput() => _buffer.ToString();
    }
}