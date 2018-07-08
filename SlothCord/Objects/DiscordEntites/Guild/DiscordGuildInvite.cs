using Newtonsoft.Json;

namespace SlothCord.Objects
{
    public struct DiscordGuildInvite
    {
        [JsonProperty("code")]
        public string Code { get; private set; }

        [JsonProperty("guild")]
        public DiscordGuild Guild { get; private set; }

        [JsonProperty("channel")]
        public DiscordGuildChannel Channel { get; private set; }

        [JsonProperty("approximate_presence_count")]
        public int? PresenceCount { get; private set; }

        [JsonProperty("approximate_member_count")]
        public int? TotalMembers { get; private set; }
    }
}