using System.Threading.Tasks;

namespace MiniRazor.Compiler.Tests.Models;

public class TestModelWithAsyncMethod
{
    public int Value1 { get; }

    public int Value2 { get; }

    public TestModelWithAsyncMethod(int value1, int value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public async Task<int> GetSumAsync() => await Task.Run(() => Value1 + Value2);
}