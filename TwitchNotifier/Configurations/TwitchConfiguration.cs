using System;

namespace TwitchNotifier.Configurations
{
    internal class TwitchConfiguration : BaseConfiguration
    {
        public override string GetSectionName() => "Twitch";

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public override void Validate()
        {
            ThrowIfStringIsEmpty(ClientId, nameof(ClientId));
            ThrowIfStringIsEmpty(ClientSecret, nameof(ClientSecret));
        }
    }
}
