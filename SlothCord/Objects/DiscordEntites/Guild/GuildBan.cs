using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct GuildBan
    {
        [JsonProperty("reason")]
        public string Reason { get; private set; }

        [JsonProperty("user")]
        public DiscordUser User { get; private set; }
    }
}
