using System.Collections.Generic;

namespace MiniRazor.Utils.Extensions
{
    internal static class StringExtensions
    {
        public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);
    }
}