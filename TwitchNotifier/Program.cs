using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TwitchNotifier;

public class Program
{
	private TwitchBot _twitchBot;
	private ConfigurationBot _config;
	private HttpClient _httpClient;

	public static Task Main(string[] args) => new Program().MainAsync();

	public async Task MainAsync()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Start");

		Console.ForegroundColor = ConsoleColor.White;
		LoadConfig();

		_httpClient = new HttpClient();

		if (_config.TimeBetweenCheck < 10)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"TimeBetweenCheck can't be under 10s");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else if (string.IsNullOrWhiteSpace(_config.DiscordBotName))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"DiscordBotName can't be null or empty");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else if (string.IsNullOrWhiteSpace(_config.DiscordBotIconUrl))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"DiscordBotIconUrl can't be null or empty");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else if (string.IsNullOrWhiteSpace(_config.TwitchClientId))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"TwitchClientId can't be null or empty");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else if (string.IsNullOrWhiteSpace(_config.TwitchSecret))
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"TwitchSecret can't be null or empty");
			Console.ForegroundColor = ConsoleColor.White;

			Console.ReadKey();
		}
		else
		{
			try
			{
				await SendMessageStartedToDiscordAsync();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.White;
			}

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
			await SendMessageToDiscordAsync(stream);
		}
		catch(Exception ex)
        {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}

	private Task SendMessageToDiscordAsync(TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, _config.DiscordUrl);

		var image = stream.ThumbnailUrl.Replace("{width}", "440").Replace("{height}", "248");

		request.Content = JsonContent.Create(new
		{
			username = _config.DiscordBotName,
			avatar_url = _config.DiscordBotIconUrl,
			content = $"**{stream.UserName}** start streaming **{stream.GameName}**",
			embeds = new List<dynamic>()
			{
				new
				{
					title = stream.Title,
					description = $"{stream.ViewerCount} viewers",
					author = new {
						name = stream.UserName,
						url = $"https://www.twitch.tv/{stream.UserName}",
						icon_url = "https://cdn.discordapp.com/embed/avatars/0.png"
					},
					timestamp = stream.StartedAt.ToString("o"),
					image = new
					{
						url = image
					}
				}
			}
		});

		Console.WriteLine("Send message to Discord !");

		return _httpClient.SendAsync(request);
	}

	private Task SendMessageStartedToDiscordAsync()
	{
		var request = new HttpRequestMessage(HttpMethod.Post, _config.DiscordUrl);

		request.Content = JsonContent.Create(new
		{
			username = _config.DiscordBotName,
			avatar_url = _config.DiscordBotIconUrl,
			content = $"Bot started for game **{_config.GameName}** !",
		});

		Console.WriteLine("Send message to Discord !");

		return _httpClient.SendAsync(request);
	}
}
