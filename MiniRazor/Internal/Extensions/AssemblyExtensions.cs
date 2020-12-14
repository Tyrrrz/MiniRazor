using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MiniRazor.Internal.Extensions
{
    internal static class AssemblyExtensions
    {
        public static Assembly Load(this AssemblyName assemblyName) =>
            Assembly.Load(assemblyName);

        public static Assembly? TryLoad(this AssemblyName assemblyName)
        {
            try
            {
                return assemblyName.Load();
            }
            catch
            {
                return null;
            }
        }

        private static void PopulateTransitiveAssemblies(this Assembly assembly, ISet<AssemblyName> assemblyNames)
        {
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                // Already exists
                if (!assemblyNames.Add(referencedAssemblyName))
                    continue;

                var referencedAssembly = referencedAssemblyName.TryLoad();
                if (referencedAssembly != null)
                    referencedAssembly.PopulateTransitiveAssemblies(assemblyNames);
            }
        }

        public static IEnumerable<AssemblyName> GetTransitiveDependencies(this Assembly assembly)
        {
            var assemblyNames = new HashSet<AssemblyName>(AssemblyNameEqualityComparer.Instance);
            assembly.PopulateTransitiveAssemblies(assemblyNames);

            return assemblyNames;
        }

        public static MetadataReference ToMetadataReference(this Assembly assembly) =>
            MetadataReference.CreateFromFile(assembly.Location);
    }
}