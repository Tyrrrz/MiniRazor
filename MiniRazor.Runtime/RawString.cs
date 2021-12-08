using System.Diagnostics.CodeAnalysis;

namespace MiniRazor;

/// <summary>
/// Contains a string value that is not meant to be encoded by the template renderer.
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