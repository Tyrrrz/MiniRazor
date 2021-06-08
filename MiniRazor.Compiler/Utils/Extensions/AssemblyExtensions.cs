using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace MiniRazor.Utils.Extensions
{
    internal static class AssemblyExtensions
    {
        public static MetadataReference ToMetadataReference(this Assembly assembly) =>
            MetadataReference.CreateFromFile(assembly.Location);

        public static Assembly? TryLoadFromAssemblyName(this AssemblyLoadContext loadContext, AssemblyName name)
        {
            try
            {
                return loadContext.LoadFromAssemblyName(name);
            }
            catch
            {
                return null;
            }
        }
    }
}