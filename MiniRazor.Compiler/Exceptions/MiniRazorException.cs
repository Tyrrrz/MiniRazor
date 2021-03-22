using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MiniRazor.Exceptions
{
    /// <summary>
    /// Exception thrown when an attempt to compile a Razor template fails.
    /// </summary>
    public partial class MiniRazorException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorException"/>.
        /// </summary>
        public MiniRazorException(string message) : base(message) {}
    }

    public partial class MiniRazorException
    {
        internal static MiniRazorException CompilationFailed(
            string generatedCode,
            IReadOnlyList<Diagnostic> diagnostics)
        {
            var buffer = new StringBuilder();

            buffer.AppendLine("Could not compile template. See below for a list of errors.");
            buffer.AppendLine();

            buffer.AppendLine("## Errors:");
            buffer.AppendJoin(Environment.NewLine, diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Select(d => d.ToString())
            );
            buffer.AppendLine();
            buffer.AppendLine();

            buffer.AppendLine("## Generated source code:");
            buffer.AppendLine(generatedCode);

            return new MiniRazorException(buffer.ToString());
        }
    }
}