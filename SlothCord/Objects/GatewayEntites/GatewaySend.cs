using Newtonsoft.Json;
using System.Collections.Generic;

namespace SlothCord.Objects
{
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

    internal struct ResumePayload
    {
        [JsonProperty("s")]
        public int Sequence { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }
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

        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMute { get; set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDeaf { get; set; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ChannelId { get; set; }
    }

    internal struct MessageUpdatePayload
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("embed")]
        public DiscordEmbed Embed { get; set; }
    }

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
        public IEnumerable<GuildChannelOverwrite> PermissionOverwrites { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }
}
