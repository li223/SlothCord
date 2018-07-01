using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SlothCord.Objects
{
    internal struct GatewayEvent
    {
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }

        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int? Sequence { get; set; }

        [JsonProperty("op")]
        public OPCode Code { get; set; }

        [JsonProperty("d")]
        public object EventPayload { get; set; }
    }

    internal struct GatewayClose
    {
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }
        
        [JsonProperty("op")]
        public CloseCode Code { get; set; }

        [JsonProperty("d")]
        public string Payload { get; set; }
    }

    internal struct ReadyPayload
    {
        [JsonProperty("v")]
        public int Version { get; set; }

        [JsonProperty("user_settings")]
        public UserSettings UserSettings { get; set; }

        [JsonProperty("user")]
        public DiscordUser User { get; set; }

        [JsonProperty("shard")]
        public int[] Shard { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("relationships")]
        public string[] Relationships { get; set; }

        [JsonProperty("private_channnels")]
        public IEnumerable<DiscordChannel> PrivateChannels { get; set; }

        [JsonProperty("guilds")]
        public IEnumerable<DiscordGuild> Guilds { get; set; }

        [JsonProperty("_trace")]
        public string[] Trace { get; set; }
    }

    internal struct UserSettings { }

    internal struct ChannelPinPayload
    {
        [JsonProperty("channel_id")]
        public ulong ChnanelId { get; set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset LastPinTimestamp { get; set; }
    }

    internal struct VoiceStateUpdatePaylod
    {
        [JsonProperty("guild_id")]
        public ulong? GuildId { get; set; }

        [JsonProperty("channel_id")]
        public ulong? ChannelId { get; set; }

        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("deaf")]
        public bool IsDeaf { get; set; }

        [JsonProperty("mute")]
        public bool IsMute { get; set; }

        [JsonProperty("self_deaf")]
        public bool IsSelfDeaf { get; set; }

        [JsonProperty("self_mute")]
        public bool IsSelfMute { get; set; }

        [JsonProperty("suppress")]
        public bool IsMutedByCurrentUser { get; set; }
    }

    internal struct PresencePayload
    {
        [JsonProperty("user")]
        public DiscordUser User { get; set; }

        [JsonProperty("status")]
        public StatusType Status { get; set; }

        [JsonProperty("roles")]
        public IEnumerable<ulong> RoleIds { get; set; }

        [JsonProperty("nick")]
        public string Nickname { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("game")]
        public DiscordActivity Activity { get; set; }
    }

    internal struct TypingStartPayload
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }
    }
    
    internal struct GatewayHello
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval { get; set; }

        [JsonProperty("_trace")]
        public IEnumerable<string> Trace { get; set; }
    }

    internal class Properties
    {
        [JsonProperty("$os")]
        public string OS { get; set; } = Environment.OSVersion.VersionString;

        [JsonProperty("$browser")]
        public string Browser { get; set; } = "SlothCord";

        [JsonProperty("device")]
        public string Device { get; set; } = "SlothCord";
    }
}
