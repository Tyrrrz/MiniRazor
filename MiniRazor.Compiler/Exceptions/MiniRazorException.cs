using System;

namespace MiniRazor.Exceptions
{
    /// <summary>
    /// Exception thrown when an attempt to compile a Razor template fails.
    /// </summary>
    public class MiniRazorException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MiniRazorException"/>.
        /// </summary>
        public MiniRazorException(string message) : base(message) {}
    }
}