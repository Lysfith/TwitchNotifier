using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TwitchNotifier.Configurations;
using TwitchNotifier.Interfaces;
using TwitchNotifier.Services;

public class Program
{
    private ILogger _logger;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        _logger.Information("Start");

        try
        {
            var services = new ServiceCollection();

            LoadConfig(services);

            services.AddSingleton(_logger);
            services.AddSingleton<IDiscordService, DiscordService>();
            services.AddSingleton<ITwitchService, TwitchService>();
            services.AddSingleton<IBotService, BotService>();

            var serviceProvider = services.BuildServiceProvider();

            var twitchService = serviceProvider.GetRequiredService<ITwitchService>();
            var discordService = serviceProvider.GetRequiredService<IDiscordService>();
            var botService = serviceProvider.GetRequiredService<IBotService>();

            var tokenSource = new System.Threading.CancellationTokenSource();
            await botService.RunAsync(tokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
        }
    }

    private void LoadConfig(ServiceCollection services)
    {
        _logger.Information("Loading config...");

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.local.json")
            .Build();

        var discordConfig = new DiscordConfiguration();
        config.Bind(discordConfig.GetSectionName(), discordConfig);
        services.AddSingleton(discordConfig);

        discordConfig.Validate();

        var twitchConfig = new TwitchConfiguration();
        config.Bind(twitchConfig.GetSectionName(), twitchConfig);
        services.AddSingleton(twitchConfig);

        twitchConfig.Validate();

        var botConfig = new BotConfiguration();
        config.Bind(botConfig.GetSectionName(), botConfig);
        services.AddSingleton(botConfig);

        botConfig.Validate();

        _logger.Information("Config loaded!");
    }

    //private async Task TwitchBot_OnNewStreamAsync(
    //    TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream
    //)
    //{
    //    try
    //    {
    //        await SendMessageToDiscordAsync(stream);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.ForegroundColor = ConsoleColor.Red;
    //        Console.WriteLine(ex.Message);
    //        Console.ForegroundColor = ConsoleColor.White;
    //    }
    //}

    //private async Task SendMessageToDiscordAsync(
    //    TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream
    //)
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Post, _config.DiscordUrl);

    //    var image = stream.ThumbnailUrl.Replace("{width}", "440").Replace("{height}", "248");

    //    request.Content = JsonContent.Create(
    //        new
    //        {
    //            username = _config.DiscordBotName,
    //            avatar_url = _config.DiscordBotIconUrl,
    //            content = $"**{stream.UserName}** start streaming **{stream.GameName}**",
    //            tts = false,
    //            embeds = new List<dynamic>()
    //            {
    //                new
    //                {
    //                    type = "rich",
    //                    title = stream.Title,
    //                    description = $"Viewers: {stream.ViewerCount} | Language: {stream.Language} | Mature: {(stream.IsMature ? "yes" : "no")}",
    //                    url = $"https://www.twitch.tv/{stream.UserLogin}",
    //                    author = new
    //                    {
    //                        name = stream.UserName,
    //                        url = $"https://www.twitch.tv/{stream.UserLogin}",
    //                        icon_url = "https://cdn.discordapp.com/embed/avatars/0.png",
    //                    },
    //                    timestamp = stream.StartedAt.ToString("o"),
    //                    image = new { url = image },
    //                },
    //            },
    //        }
    //    );

    //    Console.WriteLine("Send message to Discord !");

    //    using var _httpClient = new HttpClient();
    //    await _httpClient.SendAsync(request);
    //}

    //private async Task SendMessageStartedToDiscordAsync()
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Post, _config.DiscordUrl);

    //    request.Content = JsonContent.Create(
    //        new
    //        {
    //            username = _config.DiscordBotName,
    //            avatar_url = _config.DiscordBotIconUrl,
    //            content = $"Bot started for game **{_config.GameName}** !",
    //        }
    //    );

    //    Console.WriteLine("Send message to Discord !");

    //    using var _httpClient = new HttpClient();
    //    await _httpClient.SendAsync(request);
    //}
}
