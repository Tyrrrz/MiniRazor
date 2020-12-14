namespace MiniRazor.CodeGen.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string str) =>
            !string.IsNullOrWhiteSpace(str) ? str : null;
    }
}