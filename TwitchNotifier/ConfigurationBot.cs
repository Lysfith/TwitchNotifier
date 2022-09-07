namespace TwitchNotifier
{
    public class ConfigurationBot
    {
        public string DiscordToken { get; set; }
        public ulong DiscordChannelId { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchSecret { get; set; }
        public string GameName { get; set; }
        public int TimeBetweenCheck { get; set; }
    }
}
