using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TwitchNotifier.Interfaces
{
    internal interface IDiscordService
    {
        Task PublishStreamAsync(Stream stream);

        Task PublishMessageAsync(string message);
    }
}
