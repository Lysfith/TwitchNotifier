using System;

namespace TwitchNotifier.Configurations
{
    internal class BotConfiguration : BaseConfiguration
    {
        public override string GetSectionName() => "Bot";

        public int TimeBetweenCheck { get; set; } = 10;
        public string GameName { get; set; } = string.Empty;
        public bool PublishBotStatus { get; set; } = false;

        public override void Validate()
        {
            ThrowIfIntIsInferiorAt(TimeBetweenCheck, 10, nameof(TimeBetweenCheck));
            ThrowIfStringIsEmpty(GameName, nameof(GameName));
        }
    }
}
