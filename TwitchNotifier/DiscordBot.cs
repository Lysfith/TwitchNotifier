using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace TwitchNotifier
{
    public class DiscordBot
    {
        private DiscordSocketClient _client;
        private string _token;
        private ulong _channelId;
        private IMessageChannel _channel;

        public DiscordBot(string token, ulong channelId)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.None
            });
            _client.Log += Log;

            _token = token;
            _channelId = channelId;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine($"[{nameof(DiscordBot)}] {msg.ToString()}");
            return Task.CompletedTask;
        }


        public async Task StartAsync()
        {
            try
            {
                await _client.LoginAsync(TokenType.Bot, _token);
                await _client.StartAsync();

                _channel = _client.GetChannel(_channelId) as IMessageChannel;

                if(_channel == null)
                {
                    throw new Exception($"[{nameof(DiscordBot)}] Channel {_channelId} not found!");
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public async Task SendMessageAsync(string text)
        {
            if (_channel == null)
            {
                return; 
            }

            await _channel.SendMessageAsync(text);
        }
    }
}
