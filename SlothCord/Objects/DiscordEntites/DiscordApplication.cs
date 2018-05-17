using Newtonsoft.Json;

namespace SlothCord.Objects.DiscordEntites
{
    public struct DiscordApplication
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("bot_public")]
        public bool IsPublicBot { get; private set; }

        [JsonProperty("bot_require_code_grant")]
        public bool RequiresCodeGrant { get; private set; }

        [JsonProperty("owner")]
        public DiscordUser Owner { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("icon")]
        private string Icon { get; set; }

        [JsonIgnore]
        public string IconUrl { get => $"https://cdn.discordapp.com/icons/{this.Id}/{this.Icon}.png"; }
    }
}