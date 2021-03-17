using System.IO;
using System.Threading.Tasks;

namespace MiniRazor
{
    // Non-generic template handle for easier consumption from reflection
    public interface ITemplate
    {
        TextWriter? Output { get; set; }

        object? Model { get; set; }

        Task ExecuteAsync();
    }
}