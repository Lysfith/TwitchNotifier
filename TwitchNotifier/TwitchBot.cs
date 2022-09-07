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
        private List<string> _previousStreamIds;
        private bool _firstPass;
        private int _timeBetweenCheck;

        public event Func<Stream, Task> OnNewStreamAsync;

        public TwitchBot(string clientId, string secret, string gameName, int timeBetweenCheck)
        {
            _gameName = gameName;
            _previousStreamIds = new List<string>();
            _firstPass = true;
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
            }, accessToken: accessToken, first: STREAM_COUNT);

            return streams.Streams;
        }

        private async Task ProcessStreamsAsync(IEnumerable<Stream> streams)
        {
            if (!_firstPass)
            {
                foreach (var stream in streams)
                {
                    if (!_previousStreamIds.Contains(stream.Id))
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

            _firstPass = false;
            _previousStreamIds = streams.Select(s => s.Id).ToList();
        }
    }
}
