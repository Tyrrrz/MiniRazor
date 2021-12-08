using System.Threading;
using System.Threading.Tasks;

namespace MiniRazor.Compiler.Tests.Models;

public class TestModelWithAsyncMethodWithCancellation
{
    public async Task WaitIndefinitelyAsync(CancellationToken cancellationToken = default) =>
        await Task.Delay(-1, cancellationToken);
}