using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TwitchNotifier.Interfaces
{
    internal interface ITwitchService
    {
        Task<IEnumerable<Stream>> GetStreamsAsync(string gameName);
    }
}
