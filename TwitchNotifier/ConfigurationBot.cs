namespace TwitchNotifier
{
    public class ConfigurationBot
    {
        public string DiscordUrl { get; set; }
        public string DiscordBotName { get; set; }
        public string DiscordBotIconUrl { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchSecret { get; set; }
        public string GameName { get; set; }
        public int TimeBetweenCheck { get; set; }
    }
}
