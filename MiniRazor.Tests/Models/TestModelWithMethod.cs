namespace MiniRazor.Tests.Models
{
    public class TestModelWithMethod
    {
        public int Value1 { get; }

        public int Value2 { get; }

        public TestModelWithMethod(int value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public int GetSum() => Value1 + Value2;
    }
}