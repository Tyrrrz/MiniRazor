using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniRazor.Primitives;

namespace MiniRazor
{
    /// <summary>
    /// Base template.
    /// </summary>
    public abstract class MiniRazorTemplateBase
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        private string? _lastAttributeSuffix;

        /// <summary>
        /// Template model.
        /// </summary>
        protected dynamic? Model { get; private set; }

        internal void SetModel(object? model) => Model = model;

        /// <summary>
        /// Writes a raw literal string.
        /// </summary>
        protected void WriteLiteral(string? literal = null)
        {
            if (literal != null)
                _buffer.Append(literal);
        }

        /// <summary>
        /// Writes an encoded string.
        /// </summary>
        protected void Write(string? str = null)
        {
            if (str != null)
                WriteLiteral(WebUtility.HtmlEncode(str));
        }

        /// <summary>
        /// Writes an object.
        /// </summary>
        protected void Write(object? obj = null)
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
        /// Begins writing attribute.
        /// </summary>
        protected void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
        {
            _lastAttributeSuffix = suffix;
            WriteLiteral(prefix);
        }

        /// <summary>
        /// Writes attribute value.
        /// </summary>
        protected void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral)
        {
            WriteLiteral(prefix);
            Write(value);
        }

        /// <summary>
        /// Ends writing attribute.
        /// </summary>
        protected void EndWriteAttribute()
        {
            if (_lastAttributeSuffix == null)
                return;

            WriteLiteral(_lastAttributeSuffix);
            _lastAttributeSuffix = null;
        }

        /// <summary>
        /// Wraps a string into a container that instructs the renderer to avoid encoding.
        /// </summary>
        protected RawString Raw(string value) => new RawString(value);

        /// <summary>
        /// Executes the template.
        /// </summary>
        public abstract Task ExecuteAsync();

        internal string GetOutput() => _buffer.ToString();
    }

    /// <summary>
    /// Generic version of <see cref="MiniRazorTemplateBase"/>.
    /// </summary>
    public abstract class MiniRazorTemplateBase<T> : MiniRazorTemplateBase
    {
        /// <summary>
        /// Template model.
        /// </summary>
        public new T Model => (T) (object) base.Model!;
    }
}