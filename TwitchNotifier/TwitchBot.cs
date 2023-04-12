using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TwitchNotifier
{
    public class TwitchBot
    {
        private const int STREAM_COUNT = 100;

        private TwitchAPI _api;
        private string _gameName;
        private int _timeBetweenCheck;

        public event Func<Stream, Task> OnNewStreamAsync;

        public TwitchBot(string clientId, string secret, string gameName, int timeBetweenCheck)
        {
            _gameName = gameName;
            _timeBetweenCheck = timeBetweenCheck;

            _api = new TwitchAPI();
            _api.Settings.ClientId = clientId;
            _api.Settings.Secret = secret;
        }

        public async Task StartAsync()
        {
            while (true)
            {
                try
                {
                    var streams = await GetStreamsAsync(_gameName);
                    await ProcessStreamsAsync(streams);

                    Console.WriteLine($"[{nameof(TwitchBot)}] {streams.Count()} streams found");
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                await Task.Delay(_timeBetweenCheck * 1000);
            }
        }

        private async Task<IEnumerable<Stream>> GetStreamsAsync(string gameName)
        {
            var accessToken = await _api.Helix.Games.GetAccessTokenAsync();
            var games = await _api.Helix.Games.GetGamesAsync(null, new List<string>()
            {
                gameName
            }, accessToken);

            if (!games.Games.Any())
            {
                throw new Exception($"[{nameof(TwitchBot)}] Game {gameName} not found!");
            }

            var game = games.Games.First();

            var streams = await _api.Helix.Streams.GetStreamsAsync(gameIds: new List<string>()
            {
                game.Id
            }, accessToken: accessToken, first: STREAM_COUNT, type: "live");

            return streams.Streams;
        }

        private async Task ProcessStreamsAsync(IEnumerable<Stream> streams)
        {
            var lastStream = streams.OrderByDescending(s => s.StartedAt).FirstOrDefault();
            if (lastStream != null)
            {
                Console.WriteLine($"[{nameof(TwitchBot)}] Last stream started at: {lastStream.StartedAt.ToLocalTime()}");
            }
            var streamsStartedBetweenCheck = streams.Where(s =>
                s.GameName == _gameName
                && s.StartedAt > DateTime.UtcNow.AddSeconds(-_timeBetweenCheck))
                .ToList();

            foreach (var stream in streamsStartedBetweenCheck)
            {
                try
                {
                    await OnNewStreamAsync(stream);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(TwitchBot)}] {ex.Message}");
                }
            }
        }
    }
}
