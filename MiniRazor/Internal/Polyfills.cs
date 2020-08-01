// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0
namespace System
{
    using Collections.Generic;
    using Text;

    internal static class Extensions
    {
        public static bool Contains(this string str, string sub, StringComparison comparison) =>
            str.IndexOf(sub, comparison) >= 0;

        public static StringBuilder AppendJoin<T>(this StringBuilder stringBuilder, string separator, IEnumerable<T> values) =>
            stringBuilder.Append(string.Join(separator, values));
    }
}
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Runtime.Loader
{
    using Diagnostics.CodeAnalysis;
    using IO;
    using Reflection;

    // Fake ALC for older platforms. Doesn't do anything, but helps maintain the same API surface.
    internal class AssemblyLoadContext
    {
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        public AssemblyLoadContext(string? name, bool isCollectible = false)
        {
        }

        protected AssemblyLoadContext(bool isCollectible) : this(null, isCollectible)
        {
        }

        protected AssemblyLoadContext() : this(false)
        {
        }

        protected virtual Assembly? Load(AssemblyName name) => null;

        private static byte[] GetBytes(Stream stream)
        {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);

            return buffer.ToArray();
        }

        public Assembly LoadFromStream(Stream stream)
        {
            var data = GetBytes(stream);
            return Assembly.Load(data);
        }

        // Can't do anything :(
        public void Unload()
        {
        }
    }
}
#endif