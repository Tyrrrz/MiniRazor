using System;

using MiniRazor.CodeGen.DemoPkg.Templates;

namespace MiniRazor.CodeGen.DemoPkg
{
    class Program
    {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Console.WriteLine(SomeRzr.RenderAsync("null").Result);
        }
    }
}
