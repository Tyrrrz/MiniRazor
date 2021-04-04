using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using MiniRazor.Utils.Extensions;

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
            var errors = diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToArray();

            var message = $@"
Failed to compile template.

Error(s):
{errors.Select(m => "- " + m).JoinToString(Environment.NewLine)}

Generated source code:
{generatedCode}";

            return new MiniRazorException(message.Trim());
        }
    }
}