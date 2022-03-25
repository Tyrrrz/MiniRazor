using System.Runtime.Loader;

namespace MiniRazor;

/// <summary>
/// Options to control the behavior of the Razor compilation.
/// </summary>
public class RazorCompileOptions
{
    /// <summary>
    /// By default MiniRazor will add implicit references and use a traversal technique to find the required assemblies.
    /// Setting this to true will instead load all assemblies in the provided (else Default) <see cref="AssemblyLoadContext"/>.
    /// </summary>
    public bool InferReferencesFromAssemblyLoadContext { get; set; } = false;
}