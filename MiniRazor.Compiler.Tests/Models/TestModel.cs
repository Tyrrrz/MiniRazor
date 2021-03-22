namespace MiniRazor.Compiler.Tests.Models
{
    public class TestModel
    {
        public string Foo { get; }

        public int Number { get; }

        public TestModel(string foo, int number)
        {
            Foo = foo;
            Number = number;
        }
    }
}