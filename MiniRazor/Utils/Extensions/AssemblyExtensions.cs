using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MiniRazor.Utils.Extensions
{
    internal static class AssemblyExtensions
    {
        public static MetadataReference ToMetadataReference(this Assembly assembly) =>
            MetadataReference.CreateFromFile(assembly.Location);
    }
}