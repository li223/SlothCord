using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SlothCord.Objects
{
    internal struct ChannelModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNsfw { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ChannelOverwrite> PermissionOverwrites { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }

    internal struct HttpPayload
    {
        [JsonProperty("url")]
        public string WSUrl { get; set; }

        [JsonProperty("shards")]
        public int RecommendedShards { get; set; }
    }

    internal struct GatewayPayload
    {
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }

        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int? Sequence { get; set; }

        [JsonProperty("op")]
        public int Code { get; set; }

        [JsonProperty("d")]
        public object EventPayload { get; set; }
    }

    internal struct GatewayHello
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval { get; set; }

        [JsonProperty("_trace")]
        public IReadOnlyList<string> Trace { get; set; }
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

    internal struct PresencePayload
    {
        [JsonProperty("user")]
        public DiscordUser User { get; set; }

        [JsonProperty("status")]
        public StatusType Status { get; set; }

        [JsonProperty("roles")]
        public IReadOnlyList<ulong> RoleIds { get; set; }

        [JsonProperty("nick")]
        public string Nickname { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivity Activity { get; set; }
    }

    internal struct VoiceStateUpdatePaylod
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

    internal struct ResumePayload
    {
        [JsonProperty("s")]
        public int Sequence { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }
    }

    internal struct IdentifyPayload
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("compress")]
        public bool Compress { get; set; }

        [JsonProperty("large_threashold")]
        public int LargeThreashold { get; set; }

        [JsonProperty("shard")]
        public int[] Shard { get; set; }

        [JsonProperty("presence")]
        public DiscordPresence Presence { get; set; }

    }

    internal struct ChannelPinPayload
    {
        [JsonProperty("channel_id")]
        public ulong ChnanelId { get; set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset LastPinTimestamp { get; set; }
    }

    internal struct MessageUpdatePayload
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("embed")]
        public DiscordEmbed Embed { get; set; }
    }

    internal struct BulkDeletePayload
    {
        [JsonProperty("messages")]
        public ulong[] Messages { get; set; }
    }

    internal struct MemberModifyPayload
    {
        [JsonProperty("nick")]
        public string Nickname { get; set; }

        [JsonProperty("roles")]
        public IEnumerable<ulong> Roles { get; set; }

        [JsonProperty("mute" , NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMute { get; set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDeaf { get; set; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ChannelId { get; set; }
    }

    internal struct UserSettings { }

    internal class Properties
    {
        [JsonProperty("$os")]
        public string OS { get; set; } = Environment.OSVersion.VersionString;

        [JsonProperty("$browser")]
        public string Browser { get; set; } = "SlothCord";

        [JsonProperty("device")]
        public string Device { get; set; } = "SlothCord";
    }

    internal class UserPresencePayload
    {
        [JsonProperty("since", NullValueHandling = NullValueHandling.Include)]
        public ulong? Since { get; set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Include)]
        public DiscordGame Game { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("afk")]
        public bool Afk { get; set; } = false;
    }

    internal class MessageCreatePayload
    {
        [JsonProperty("has_content", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasContent { get; set; } = null;

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; } = null;

        [JsonProperty("is_tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; } = null;

        [JsonProperty("has_embed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasEmbed { get; set; } = null;

        [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbed Embed { get; set; } = null;

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] FileData { get; set; } = null;

        [JsonProperty("payload_json", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonPayload { get; set; } = null;
    }

    internal class ReadyPayload
    {
        [JsonProperty("v")]
        public int Version { get; set; }

        [JsonProperty("user_settings", NullValueHandling = NullValueHandling.Ignore)]
        public UserSettings UserSettings { get; set; }

        [JsonProperty("user")]
        public DiscordUser User { get; set; }

        [JsonProperty("shard")]
        public int[] Shard { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Relationships { get; set; }

        [JsonProperty("private_channnels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> PrivateChannels { get; set; }

        [JsonProperty("guilds")]
        public IReadOnlyList<DiscordGuild> Guilds { get; set; }

        [JsonProperty("_trace")]
        public string[] Trace { get; set; }
    }
}
