using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MiniRazor.Exceptions
{
    /// <summary>
    /// Exception which is thrown when an attempt to compile a Razor template has failed.
    /// </summary>
    public partial class RazorCompilationException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="RazorCompilationException"/>.
        /// </summary>
        public RazorCompilationException(string message) : base(message) { }
    }

    public partial class RazorCompilationException
    {
        internal static RazorCompilationException FromDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
        {
            var buffer = new StringBuilder();

            buffer.AppendLine("Could not compile template.");
            buffer.AppendLine();

            buffer.AppendJoin(Environment.NewLine, diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Select(d => d.ToString())
            );

            return new RazorCompilationException(buffer.ToString());
        }
    }
}