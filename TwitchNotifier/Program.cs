using System;
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
            .AddJsonFile("appsettings.local.json", true)
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
}
