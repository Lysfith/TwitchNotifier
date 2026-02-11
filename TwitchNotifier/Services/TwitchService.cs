using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchNotifier.Configurations;
using TwitchNotifier.Interfaces;

namespace TwitchNotifier.Services
{
    internal class TwitchService : ITwitchService
    {
        private const int STREAM_COUNT = 100;

        private readonly ILogger _logger;
        private readonly TwitchConfiguration _config;
        private readonly TwitchAPI _api;

        public TwitchService(TwitchConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;

            _api = new TwitchAPI();
            _api.Settings.ClientId = _config.ClientId;
            _api.Settings.Secret = _config.ClientSecret;
        }

        public async Task<IEnumerable<Stream>> GetStreamsAsync(string gameName)
        {
            var accessToken = await _api.Helix.Games.GetAccessTokenAsync();
            var games = await _api.Helix.Games.GetGamesAsync(
                null,
                new List<string>() { gameName },
                accessToken
            );

            if (!games.Games.Any())
            {
                _logger.Warning($"[{nameof(TwitchService)}] Game {gameName} not found!");
                return Enumerable.Empty<Stream>();
            }

            var game = games.Games.First();

            var streams = await _api.Helix.Streams.GetStreamsAsync(
                gameIds: new List<string>() { game.Id },
                accessToken: accessToken,
                first: STREAM_COUNT
            );

            return streams.Streams;
        }
    }
}
