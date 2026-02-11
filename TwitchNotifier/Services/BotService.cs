using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchNotifier.Configurations;
using TwitchNotifier.Interfaces;
using TwitchNotifier.Models;

namespace TwitchNotifier.Services
{
    internal class BotService : IBotService
    {
        private readonly ILogger _logger;
        private readonly BotConfiguration _config;
        private readonly ITwitchService _twitchService;
        private readonly IDiscordService _discordService;

        private List<StreamInfo> _streams;

        public BotService(
            BotConfiguration config,
            ITwitchService twitchService,
            IDiscordService discordService,
            ILogger logger
        )
        {
            _config = config;
            _logger = logger;
            _twitchService = twitchService;
            _discordService = discordService;

            _streams = new List<StreamInfo>();
        }

        public async Task RunAsync(CancellationToken token)
        {
            if (_config.PublishBotStatus)
            {
                await _discordService.PublishMessageAsync("Bot started !");
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var streams = await _twitchService.GetStreamsAsync(_config.GameName);
                    _logger.Information($"[{nameof(BotService)}] {streams.Count()} streams found");

                    await ProcessStreamsAsync(streams);

                    _streams.RemoveAll(s => s.StartedAt < DateTime.Now.AddDays(-2));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }

                await Task.Delay(_config.TimeBetweenCheck * 1000);
            }

            if (_config.PublishBotStatus)
            {
                await _discordService.PublishMessageAsync("Bot stopped !");
            }
        }

        private async Task ProcessStreamsAsync(IEnumerable<Stream> streams)
        {
            var newStreams = streams
                .Where(s => s.GameName == _config.GameName && !_streams.Any(d => d.Id == s.Id))
                .ToList();

            _logger.Information($"[{nameof(BotService)}] {newStreams.Count()} new streams");

            foreach (var stream in newStreams)
            {
                try
                {
                    _streams.Add(new StreamInfo() { Id = stream.Id, StartedAt = stream.StartedAt });
                    await _discordService.PublishStreamAsync(stream);
                }
                catch (Exception ex)
                {
                    _logger.Information($"[{nameof(BotService)}] {ex.Message}");
                }
            }
        }
    }
}
