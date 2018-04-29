using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SlothCord.Objects
{
    internal class HttpPayload
    {
        [JsonProperty("url")]
        public string WSUrl { get; internal set; }
        [JsonProperty("shards")]
        public int RecommendedShards { get; internal set; }
    }

    internal class MessageCreatePayload
    {
        [JsonProperty("has_content")]
        public bool HasContent { get; internal set; }
        [JsonProperty("content")]
        public string Content { get; internal set; }
        [JsonProperty("is_tts")]
        public bool IsTTS { get; internal set; }
        [JsonProperty("has_embed")]
        public bool HasEmbed { get; internal set; }
        [JsonProperty("embed")]
        public DiscordEmbed Embed { get; internal set; }
    }

    internal class ReadyPayload
    {
        [JsonProperty("v")]
        public int Version { get; private set; }
        [JsonProperty("user_settings", NullValueHandling = NullValueHandling.Ignore)]
        public UserSettings UserSettings { get; private set; }
        [JsonProperty("user")]
        public DiscordUser User { get; private set; }
        [JsonProperty("shard")]
        public int[] Shard { get; private set; }
        [JsonProperty("session_id")]
        public string SessionId { get; private set; }
        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Relationships { get; private set; }
        [JsonProperty("private_channnels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> PrivateChannels { get; private set; }
        [JsonProperty("guilds")]
        public IReadOnlyList<DiscordGuild> Guilds { get; private set; }
        [JsonProperty("_trace")]
        public string[] Trace { get; private set; }
    }

    internal class GatewayPayload
    {
        [JsonProperty("t")]
        public string EventName { get; set; }
        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int? Sequence { get; set; }
        [JsonProperty("op")]
        public int Code { get; set; }
        [JsonProperty("d")]
        public object EventPayload { get; set; }
    }

    internal class GatewayHello
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval { get; private set; }
        [JsonProperty("_trace")]
        public IReadOnlyList<string> Trace { get; private set; }
    }

    internal class TypingStartPayload
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; private set; }
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; private set; }
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }
    }

    internal class PresencePayload
    {
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        [JsonProperty("status")]
        public StatusType Status { get; internal set; }
        [JsonProperty("roles")]
        public IReadOnlyList<ulong> RoleIds { get; private set; }
        [JsonProperty("nick")]
        public string Nickname { get; private set; }
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivity Activity { get; internal set; }
    }

    internal class UserPresencePayload
    {
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }

        [JsonProperty("status")]
        public string Status { get; internal set; }

        [JsonProperty("game")]
        public DiscordGame Game { get; internal set; }
    }

    internal class VoiceStateUpdatePaylod
    {
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? GuildId { get; set; }
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
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

    internal class ResumePayload
    {
        [JsonProperty("s")]
        public int Sequence { get; internal set; }
        [JsonProperty("token")]
        public string Token { get; internal set; }
        [JsonProperty("session_id")]
        public string SessionId { get; internal set; }
    }

    internal class IdentifyPayload
    {
        [JsonProperty("token")]
        public string Token { get; internal set; }
        [JsonProperty("properties")]
        public Properties Properties { get; internal set; }
        [JsonProperty("compress")]
        public bool Compress { get; internal set; }
        [JsonProperty("large_threashold")]
        public int LargeThreashold { get; internal set; }
        [JsonProperty("shard")]
        public int[] Shard { get; internal set; }
        [JsonProperty("presence")]
        public DiscordPresence Presence { get; internal set; }

    }

    internal class ChannelPinPayload
    {
        [JsonProperty("channel_id")]
        public ulong ChnanelId { get; set; }
        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset LastPinTimestamp { get; set; }
    }

    internal class MessageUpdatePayload
    {
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("embed")]
        public DiscordEmbed Embed { get; set; }
    }

    internal class BulkDeletePayload
    {
        [JsonProperty("messages")]
        public ulong[] Messages { get; set; }
    }

    internal class MemberModifyPayload
    {
        [JsonProperty("nick")]
        public string Nickname { get; set; }
        [JsonProperty("roles")]
        public IEnumerable<DiscordRole> Roles { get; set; }
        [JsonProperty("mute" , NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMute { get; set; }
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDeaf { get; set; }
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ChannelId { get; set; }
    }

    internal class Properties
    {
        [JsonProperty("$os")]
        public string OS { get; private set; } = Environment.OSVersion.VersionString;
        [JsonProperty("$browser")]
        public string Browser { get; private set; } = "SlothCord";
        [JsonProperty("device")]
        public string Device { get; private set; } = "SlothCord";
    }

    internal class UserSettings { }
}
