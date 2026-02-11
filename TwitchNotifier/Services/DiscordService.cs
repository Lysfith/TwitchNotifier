using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchNotifier.Configurations;
using TwitchNotifier.Interfaces;

namespace TwitchNotifier.Services
{
    internal class DiscordService : IDiscordService
    {
        private readonly ILogger _logger;
        private readonly DiscordConfiguration _config;

        public DiscordService(DiscordConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task PublishStreamAsync(Stream stream)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _config.Url);

            var image = stream.ThumbnailUrl.Replace("{width}", "440").Replace("{height}", "248");

            request.Content = JsonContent.Create(
                new
                {
                    username = _config.BotName,
                    avatar_url = _config.BotIconUrl,
                    content = $"**{stream.UserName}** start streaming **{stream.GameName}**",
                    tts = false,
                    embeds = new List<dynamic>()
                    {
                        new
                        {
                            type = "rich",
                            title = stream.Title,
                            description = $"Viewers: {stream.ViewerCount} | Language: {stream.Language}",
                            url = $"https://www.twitch.tv/{stream.UserLogin}",
                            author = new
                            {
                                name = stream.UserName,
                                url = $"https://www.twitch.tv/{stream.UserLogin}",
                            },
                            timestamp = stream.StartedAt.ToString("o"),
                            image = new { url = image },
                        },
                    },
                }
            );

            using var _httpClient = new HttpClient();
            await _httpClient.SendAsync(request);

            _logger.Information($"[Discord] stream '{stream.UserName}' published");
        }

        public async Task PublishMessageAsync(string message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _config.Url);

            request.Content = JsonContent.Create(
                new
                {
                    username = _config.BotName,
                    avatar_url = _config.BotIconUrl,
                    content = message,
                }
            );

            using var _httpClient = new HttpClient();
            await _httpClient.SendAsync(request);

            _logger.Information($"[Discord] message '{message}' published");
        }
    }
}
