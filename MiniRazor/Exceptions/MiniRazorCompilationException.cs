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
    public partial class MiniRazorCompilationException : Exception
    {
        /// <summary>
        /// Generated code.
        /// </summary>
        public string GeneratedCode { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorCompilationException"/>.
        /// </summary>
        public MiniRazorCompilationException(string message, string generatedCode)
            : base(message)
        {
            GeneratedCode = generatedCode;
        }
    }

    public partial class MiniRazorCompilationException
    {
        internal static MiniRazorCompilationException FromDiagnostics(string generatedCode, IReadOnlyList<Diagnostic> diagnostics)
        {
            var buffer = new StringBuilder();

            buffer
                .Append("Could not compile template. ")
                .Append("See below for the list of errors. ")
                .Append($"You can also inspect the {nameof(GeneratedCode)} property to see the generated source code.")
                .AppendLine()
                .AppendLine();

            buffer.AppendJoin(Environment.NewLine, diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Select(d => d.ToString())
            );

            return new MiniRazorCompilationException(buffer.ToString(), generatedCode);
        }
    }
}