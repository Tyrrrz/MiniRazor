namespace MiniRazor.Primitives
{
    /// <summary>
    /// Wraps a string and instructs the renderer to treat it without encoding.
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
        public override string ToString() => Value;
    }
}