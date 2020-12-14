using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiniRazor.Internal
{
    internal class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
    {
        public static AssemblyNameEqualityComparer Instance { get; } = new();

        public bool Equals(AssemblyName? x, AssemblyName? y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x?.FullName, y?.FullName);

        public int GetHashCode(AssemblyName obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
    }
}