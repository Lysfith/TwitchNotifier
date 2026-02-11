using System.Threading;
using System.Threading.Tasks;

namespace TwitchNotifier.Interfaces
{
    internal interface IBotService
    {
        Task RunAsync(CancellationToken token);
    }
}
