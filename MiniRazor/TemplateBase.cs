using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MiniRazor
{
    /// <summary>
    /// Base implementation of a Razor template.
    /// </summary>
    public abstract class TemplateBase<TModel> : ITemplate
    {
        private string? _lastAttributeSuffix;

        /// <summary>
        /// Template output.
        /// </summary>
        public TextWriter? Output { get; set; }

        /// <summary>
        /// Template model.
        /// </summary>
        public TModel Model { get; set; } = default!;

        object? ITemplate.Model
        {
            get => Model;
            set => Model = (TModel) value!;
        }

        /// <summary>
        /// Wraps a string in a container that instructs the renderer to avoid encoding.
        /// </summary>
        protected RawString Raw(string value) => new RawString(value);

        /// <summary>
        /// Writes a template literal.
        /// </summary>
        protected void WriteLiteral(string? literal) => Output?.Write(literal);

        /// <summary>
        /// Writes a string with encoding.
        /// </summary>
        protected void Write(string? str)
        {
            if (str != null)
                WriteLiteral(WebUtility.HtmlEncode(str));
        }

        /// <summary>
        /// Writes a string without encoding.
        /// </summary>
        protected void Write(RawString str) => WriteLiteral(str.Value);

        /// <summary>
        /// Writes an object.
        /// </summary>
        protected void Write(object? obj)
        {
            if (obj is string str)
            {
                Write(str);
            }
            else if (obj is RawString raw)
            {
                Write(raw);
            }
            else
            {
                Write(obj?.ToString());
            }
        }

        /// <summary>
        /// Begins writing attribute.
        /// </summary>
        protected void BeginWriteAttribute(
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            _lastAttributeSuffix = suffix;
            WriteLiteral(prefix);
        }

        /// <summary>
        /// Writes attribute value.
        /// </summary>
        protected void WriteAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            WriteLiteral(prefix);
            Write(value);
        }

        /// <summary>
        /// Ends writing attribute.
        /// </summary>
        protected void EndWriteAttribute()
        {
            WriteLiteral(_lastAttributeSuffix);
            _lastAttributeSuffix = null;
        }

        /// <summary>
        /// Executes the template.
        /// </summary>
        public abstract Task ExecuteAsync();
    }
}