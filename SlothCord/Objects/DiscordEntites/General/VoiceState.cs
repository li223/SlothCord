using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct UserVoiceState
    {
        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }
        
        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        /// <summary>
        /// Is either DiscordChannel for DiscordGuildChannel
        /// </summary>
        [JsonIgnore]
        public object Channel { get; internal set; }

        [JsonProperty("user_id")]
        internal ulong UserId { get; set; }

        [JsonProperty("session_id")]
        internal string SessionId { get; set; }

        [JsonProperty("deaf")]
        public bool IsDeafened { get; private set; }

        [JsonProperty("mute")]
        public bool IsMuted { get; private set; }

        [JsonProperty("self_deaf")]
        public bool IsSelfDeafened { get; private set; }

        [JsonProperty("self_mute")]
        public bool IsSelfMuted { get; private set; }

        [JsonProperty("suppress")]
        public bool IsSuppressed { get; private set; }
    }

    public struct VoiceRegion
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("vip")]
        public bool IsVipOnly { get; private set; }

        [JsonProperty("optimal")]
        public bool IsOptimal { get; private set; }

        [JsonProperty("deprecated")]
        public bool IsDeprecated { get; private set; }

        [JsonProperty("custom")]
        public bool IsCustom { get; private set; }
    }
}