// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class PolyfillExtensions
{
    public static bool Contains(this string str, string sub, StringComparison comparison) =>
        str.IndexOf(sub, comparison) >= 0;

    public static StringBuilder AppendJoin<T>(
        this StringBuilder stringBuilder,
        string separator,
        IEnumerable<T> values) =>
        stringBuilder.Append(string.Join(separator, values));
}
#endif