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

        private List<StreamInfo> _streams;

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

            _streams = new List<StreamInfo>();
        }

        public async Task StartAsync()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"==============================================");
                    var streams = await GetStreamsAsync(_gameName);
                    Console.WriteLine($"[{nameof(TwitchBot)}] {streams.Count()} streams found");

                    await ProcessStreamsAsync(streams);

                    _streams.RemoveAll(s => s.StartedAt < DateTime.Now.AddDays(-2));
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
            var newStreams = streams
                .Where(s => s.GameName == _gameName
                    && !_streams.Any(d => d.Id == s.Id))
                .ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{nameof(TwitchBot)}] {newStreams.Count()} new streams");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (var stream in newStreams)
            {
                try
                {
                    _streams.Add(new StreamInfo()
                    {
                        Id = stream.Id,
                        StartedAt = stream.StartedAt
                    });
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

    public struct StreamInfo
    {
        public string Id { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
