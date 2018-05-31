using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct DiscordGame
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public PlayingType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
