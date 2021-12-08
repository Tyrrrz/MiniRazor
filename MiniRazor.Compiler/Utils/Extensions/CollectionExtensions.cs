using System.Collections.Generic;

namespace MiniRazor.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var i in source)
        {
            if (i is not null)
                yield return i;
        }
    }
}