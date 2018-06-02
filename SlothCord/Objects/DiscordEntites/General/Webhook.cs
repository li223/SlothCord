using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct Webhook
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        /// <summary>
        /// Either a DiscordGuildChannel or DiscordChannel
        /// </summary>
        [JsonIgnore]
        public object Channel { get; internal set; }

        [JsonProperty("user")]
        public DiscordUser CreatedBy { get; internal set; }

        [JsonIgnore]
        public DiscordGuildMember CreatedByMember { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("avatar")]
        public string Avatar { get; private set; }

        [JsonIgnore]
        public string AvatarUrl { get => $"https://cdn.discordapp.com/avatars/{this.Id}/{this.Avatar}.png"; }

        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
