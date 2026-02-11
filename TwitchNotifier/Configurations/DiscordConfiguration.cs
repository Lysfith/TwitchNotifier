using System;

namespace TwitchNotifier.Configurations
{
    internal class DiscordConfiguration : BaseConfiguration
    {
        public override string GetSectionName() => "Discord";

        public string Url { get; set; } = string.Empty;
        public string BotName { get; set; } = string.Empty;
        public string BotIconUrl { get; set; } = string.Empty;

        public override void Validate()
        {
            ThrowIfStringIsEmpty(Url, nameof(Url));
            ThrowIfStringIsEmpty(BotName, nameof(BotName));
            ThrowIfStringIsEmpty(BotIconUrl, nameof(BotIconUrl));
        }
    }
}
