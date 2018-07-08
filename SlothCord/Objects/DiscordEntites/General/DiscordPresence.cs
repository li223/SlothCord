using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct DiscordPresence
    {
        [JsonProperty("game")]
        public DiscordGame? Game { get; private set; }

        [JsonProperty("status")]
        public StatusType? Status { get; private set; }

        [JsonProperty("since")]
        public long? Since { get; private set; }

        [JsonProperty("afk")]
        public bool? AFK { get; private set; }
    }
}
