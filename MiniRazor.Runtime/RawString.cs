using System.Diagnostics.CodeAnalysis;

namespace MiniRazor
{
    /// <summary>
    /// Wraps a string and instructs the renderer to avoid encoding it.
    /// </summary>
    public readonly struct RawString
    {
        /// <summary>
        /// Underlying string value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes an instance of <see cref="RawString"/>.
        /// </summary>
        public RawString(string value) => Value = value;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Value;
    }
}