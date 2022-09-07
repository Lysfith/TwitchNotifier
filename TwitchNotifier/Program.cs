using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using TwitchNotifier;

public class Program
{
	private DiscordBot _discordBot;
	private TwitchBot _twitchBot;
	private ConfigurationBot _config;

	public static Task Main(string[] args) => new Program().MainAsync();

	public async Task MainAsync()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Start");

		Console.ForegroundColor = ConsoleColor.White;
		LoadConfig();

		if (_config.TimeBetweenCheck < 10)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else
		{
			_discordBot = new DiscordBot(_config.DiscordToken, _config.DiscordChannelId);
			await _discordBot.StartAsync();

			_twitchBot = new TwitchBot(_config.TwitchClientId, _config.TwitchSecret, _config.GameName, _config.TimeBetweenCheck);
			_twitchBot.OnNewStreamAsync += TwitchBot_OnNewStreamAsync;
			await _twitchBot.StartAsync();

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Bot started");
			Console.ForegroundColor = ConsoleColor.White;

			await Task.Delay(-1);
		}
	}

	private void LoadConfig()
    {
		Console.WriteLine("Loading config...");

		IConfiguration config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		_config = new ConfigurationBot();
		config.Bind(_config);

		Console.WriteLine("Config loaded!");
	}

	private async Task TwitchBot_OnNewStreamAsync(TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream)
	{
        try
        {
			await _discordBot.SendMessageAsync($"New stream by {stream.UserName} ({stream.Language}) with {stream.ViewerCount} viewers");
        }
		catch(Exception ex)
        {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
