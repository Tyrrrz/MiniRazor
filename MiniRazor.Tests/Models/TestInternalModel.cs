using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FriendlyAssemblyName")]

namespace MiniRazor.Tests.Models
{
    internal class TestInternalModel
    {
        public string Foo { get; }

        public TestInternalModel(string foo)
        {
            Foo = foo;
        }
    }
}